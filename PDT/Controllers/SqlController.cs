using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BAMCIS.GeoJSON;
using Newtonsoft.Json;
using Npgsql;

namespace PDT.Controllers
{
    public class SqlController : Controller
    {
        // union select name, ST_ASGeoJSON(ST_TRANSFORM(range, 4326)) from fiit_center
        public string connString = "Host=192.168.99.100;Username=martin;Password=cloud1995;Database=pdt";

        public string getPrimaryRoads = @"with village as (SELECT NAME, way FROM planet_osm_polygon  WHERE admin_level = '8'),
                                            roads as (select ST_Multi(ST_LineMerge(St_Collect(way))) as way from planet_osm_roads where highway like 'motorway')
                                        select ST_ASGeoJSON(ST_TRANSFORM(village.way,4326)) as village_polygon, ST_ASGeoJSON(ST_TRANSFORM(roads.way,4326)) as road_line from village inner join roads ON ST_INTERSECTS(roads.way, village.way)";

        public string getRangeFromFiit = @"with fiit_center as (select operator as name, ST_BUFFER(ST_Centroid(way),1000) as range from planet_osm_polygon
		                                     where building = 'university' and operator like 'Slovenská technická univerzita v Bratislave'), 
                                        cafe as (select name, way from planet_osm_point where amenity like 'cafe' or amenity like 'bar' or amenity like 'restaurant')

                                        select cafe.name, ST_ASGeoJSON(ST_TRANSFORM(cafe.way, 4326)), false as main_value from fiit_center join cafe on ST_Contains(fiit_center.range, cafe.way)
                                       
                                        union
                                        select name, ST_ASGeoJSON(ST_TRANSFORM(ST_Centroid(range), 4326)), true as main_value from fiit_center"; //fiit nema meno, iba operatora
        public string GetData()
        {
            using (var connection = new NpgsqlConnection(connString))
            {
                try
                {
                    connection.Open();

                    DataTable data = new DataTable();
                    NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(getPrimaryRoads, connection);
                    adapter.Fill(data);

                    List<String> list = new List<String>();

                    List<Feature> features = new List<Feature>();

                    Polygon villageArea;
                    MultiLineString roads = null;


                    foreach (DataRow dr in data.Rows)
                    {
                        villageArea = JsonConvert.DeserializeObject<Polygon>(dr[0].ToString());
                        features.Add(new Feature(villageArea));

                        if (roads == null)
                        {
                            roads = JsonConvert.DeserializeObject<MultiLineString>(dr[1].ToString());
                            features.Add(new Feature(roads));
                        }
                    }

                    FeatureCollection featureCollection = new FeatureCollection(features);

                    return JsonConvert.SerializeObject(featureCollection);
                }
                catch (Exception ex)
                {
                    return null;
                }
        }
    }

        public string GetRangeFromFiit()
        {
            using (var connection = new NpgsqlConnection(connString))
            {
                try
                {
                    connection.Open();

                    DataTable data = new DataTable();
                    NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(getRangeFromFiit, connection);
                    adapter.Fill(data);

                    List<String> list = new List<String>();

                    List<Feature> features = new List<Feature>();
                    Polygon radius;
                    Point point;


                    foreach (DataRow dr in data.Rows)
                    {
                        point = JsonConvert.DeserializeObject<Point>(dr[1].ToString());
                        features.Add(new Feature(point, new Dictionary<string, object> { { "Name", dr[0] }, { "Center", dr[2] } }));
                    }


                    FeatureCollection featureCollection = new FeatureCollection(features);


                    return JsonConvert.SerializeObject(featureCollection);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        //public string GetDepos()
        //{
        //    using (var connection = new NpgsqlConnection(connString))
        //    {
        //        try
        //        {
        //            connection.Open();

        //            DataTable data = new DataTable();
        //            NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(getAeroStations, connection);
        //            adapter.Fill(data);

        //            List<String> list = new List<String>();

        //            List<Feature> features = new List<Feature>();
        //            Polygon station;


        //            foreach (DataRow dr in data.Rows)
        //            {
        //                station = JsonConvert.DeserializeObject<Polygon>(dr[0].ToString());
        //                features.Add(new Feature(station));
        //            }


        //            FeatureCollection featureCollection = new FeatureCollection(features);


        //            return JsonConvert.SerializeObject(featureCollection);
        //        }
        //        catch (Exception ex)
        //        {
        //            return null;
        //        }
        //    }
        //}

        public object GetSomething(string village)
        {
            string getVillage = $@"with village as (SELECT NAME, way FROM planet_osm_polygon WHERE cast(admin_level as decimal(9,2)) between 6 and 9 and name like '{village}') 
                            select ST_ASGeoJSON(ST_TRANSFORM(way,4326)) as polygon, ST_ASGeoJSON(ST_TRANSFORM(ST_Centroid(way), 4326)) as center, ST_Area(ST_TRANSFORM(way, 4326)::geography) as area from village ";

            using (var connection = new NpgsqlConnection(connString))
            {

                try
                {
                    connection.Open();

                    DataTable data = new DataTable();
                    NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(getVillage, connection);
                    adapter.Fill(data);

                    List<String> list = new List<String>();


                    List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                    Dictionary<string, object> row;

                    List<Feature> features = new List<Feature>();

                    Polygon villageArea;
                    Point center;
                    double area = 0; 


                    foreach (DataRow dr in data.Rows)
                    {
                            villageArea = JsonConvert.DeserializeObject<Polygon>(dr[0].ToString());
                            features.Add(new Feature(villageArea));

                            center = JsonConvert.DeserializeObject<Point>(dr[1].ToString());
                            features.Add(new Feature(center));

                            area = double.Parse(dr[2].ToString());
                            //area = JsonConvert.DeserializeObject<double>(dr[2].ToString());
                    }

                    FeatureCollection featureCollection = new FeatureCollection(features);

                    return new {
                        data = JsonConvert.SerializeObject(featureCollection),
                        area
                    };
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
    }
}

