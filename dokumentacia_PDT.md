# PDT projekt
Martin Gembec

Výsledkom tohto projektu je webová aplikácia. Táto webová aplikácia je určená pre vyhľadávanie miest a obcí na mape podľa názvu obce zadaného používateľom. Taktiež je možné zvýrazniť okresy, ktoré pretína diaľnica. A nakoniec je k dispozícií vyhľadávač kaviarní a reštaurácií, kde si môžu študenti FIIT po dni v škole odskočiť na občerstvenie. 

## Použité technológie
V projekte boli použité rozličné technológie, či už pre frontend, backend alebo databázu. Celkovo bol projekt vyvíjaný vo vývojovom prostredí Visual Studio 2017.

### Frontend
Základ frontendu tvorí **HTML** spolu s **CSS**. Podporované sú programovacím jazykom **JavaScript**. V projekte sú použíté knižnice **DevExtreme** (pre TextBox, do ktorého používateľ vkladá názov obce) a **JQuery-UI** (pohyb s ovládacím panelom). Prácu s mapou zabezpečuje JavaScript-ová **Mapbox GL JS** knižnica.

### Backend
Backend je založený na programovacom jazyku **C#**, ktorý spracuje prijaté geodáta z databázy. Komunikáciu databázy s backendom zabezpečuje **Npgsql** konektor. Pre serializovanie geodát je použité JSON rozhranie **Newtonsoft**.

### Databáza
Ako databázový server bol použitý **PostgreSQL 10**. Pre podporu geodát bolo potrebné pridať ku PostgreSQL aj **PostGIS**. Databázový server bol vytvorený pomocou Docker container-a **kartoza/postgis:9.6-2.4**, ktorý hneď po vytvorení obsahoval PostGIS funkcionalitu.

#### Dáta
Zvolený bol väčší dataset, ktorý obsahuje dáta pre celé Slovensko. Dataset bol stiahnutý zo stránky OpenStreetMap.org, kontrétne zo serveru Geofabrik. Pre import datasetu do databázy bol použitý nástroj osm2pgsgl.
Importovaný dataset Slovenska obsahuje tri tabuľky:
* planet_osm_line
* planet_osm_point
* planet_osm_polygon
* planet_osm_roads

Posledný výraz v názve napovedá, aké dáta sa v tabuľke nachádzajú. Okrem geodát je možné v tabuľkách nájsť informácie o charaktere týchto dát, ako napríklad, či sa jedná o historickú pamiatku, športovú atrakciu alebo prípadne o územie parku alebo okresu. V projekte bola použítá predovšetkým tabuľka s polygónmi, avšak pri realizácií scenárov boli použíté aj zvyšné tabuľky.

#### Indexy
Boli použité indexy, konkrétne v tabuľke planet_osm_polygon indexy na stĺpce admin_level a name. Cez tieto stĺpce prebieha vyhľadávanie v scenári 1. Vytvorený bol ešte jeden index v tabuľke planet_osm_roads na stĺpci highway pre urýchlenie vyhľadávania medzi rôznymi typmi ciest.

## Opis scenárov

### Scenár 1 - Vyhľadaj obec
Prvý scenár je zameraný na vyhľadávanie obcí z celého Slovenska. Používateľ má k dispozícií okno, do ktorého môže napísať názov obce, ktorú chce vyhľadať. Je potrebné, aby bola obec zapísaná s diakritikou a s veľkými písmenami na začiatku (napr. Liptovský Mikuláš, Brezno, Moldava nad Bodvou). Následne sa vykoná query

**with village as (SELECT NAME, way FROM planet_osm_polygon 
			WHERE cast(admin_level as decimal(9,2)) between 6 and 9 and name like '{village}')** 
	
**select ST_ASGeoJSON(ST_TRANSFORM(way,4326)) as polygon, ST_ASGeoJSON(ST_TRANSFORM(ST_Centroid(way), 4326)) as center, 
                            ST_Area(ST_TRANSFORM(way, 4326)::geography) as area from village**

kde {village} predstavuje zadaný názov obce. následne sa nájde obec s hľadaným názvom, vyberú sa geodáta pre polygón, vypočíta sa plocha, ktorú polygón zaberá pomocou funkcie ST_AREA a vypočíta sa stred tohto polygónu. Stred je následne použitý pre umiestnenie značky, na ktorú je možné kliknúť pre zobrazenie rozlohy danej obce.

![image](https://github.com/gembecmartin/assignment-gis/blob/testBranch/images/scenar1.PNG)

### Scenár 2 - Zobraz okresy s diaľnicou
V tomto scenári sa po kliknutí na tlačidlo zvýraznia na maper všetky okresy, ktorým cez územie prechádza diaľnica. Samotné diaľnice sú následne zvýraznené čiernou farbou, pre možnosť overenia správnosti výsledku. Prienik diaľnice a okresu sa dá jednoducho určiť pomocou funkcie ST_INTERSECTS. Pre získanie tohto výsledku použijeme query: 

**with village as (SELECT NAME, way FROM planet_osm_polygon  WHERE admin_level = '8'),
roads as (select ST_Multi(ST_LineMerge(St_Collect(way))) as way from planet_osm_roads 
where highway like 'motorway')**

**select ST_ASGeoJSON(ST_TRANSFORM(village.way,4326)) as village_polygon, ST_ASGeoJSON(ST_TRANSFORM(roads.way,4326)) as road_line from village inner join roads 
ON ST_INTERSECTS(roads.way, village.way)**

![image](https://github.com/gembecmartin/assignment-gis/blob/testBranch/images/scenar2.PNG)

### Scenár 3 - Zobraz podniky v okolí FIIT
Posledný scenár slúži pre vyhľadávanie podnikov pre študentov FIIT, kde sa môžu po náročnom dni v škole občerstviť. Podniky sú vyhľadávané v radiuse, ktorý si zvolí používateľ v okne výberu, od stredu fakulty, ktorá sa vypočíta funkciou ST_Centroid. K dispozícií sú 3 možné rozmedzia Query vyzerá nasledovne:

**with 
fiit_center as (select operator as name, ST_BUFFER(ST_Centroid(way),1000) as range from planet_osm_polygon where building = 'university' and operator like 'Slovenská technická univerzita v Bratislave'), 
                                        cafe as (select name, way from planet_osm_point where amenity like 'cafe' or amenity like 'bar' or amenity like 'restaurant')**

**select `cafe.name`, ST_ASGeoJSON(ST_TRANSFORM(cafe.way, 4326)), false as main_value 
from fiit_center join cafe on ST_Contains(fiit_center.range, cafe.way)
                                        union
                                        select name, ST_ASGeoJSON(ST_TRANSFORM(ST_Centroid(range), 4326)), true as main_value from fiit_center**

![image](https://github.com/gembecmartin/assignment-gis/blob/testBranch/images/scenar3.PNG)

### Bonus - Pohyblivý ovládací panel
Používateľ môže pomocou potiahnutia ovládacieho panelu premiestniť ovládací panel v prípade, že by mu na aktuálnej pozícií prekážal.
