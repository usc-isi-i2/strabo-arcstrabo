/*******************************************************************************
 * Copyright 2010 University of Southern California
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * 	http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * This code was developed as part of the Strabo map processing project 
 * by the Spatial Sciences Institute and by the Information Integration Group 
 * at the Information Sciences Institute of the University of Southern 
 * California. For more information, publications, and related projects, 
 * please see: http://yoyoi.info and http://www.isi.edu/integration
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using com.hp.hpl.jena.query;
using com.hp.hpl.jena.tdb;
using com.hp.hpl.jena.rdf.model;
using System;


namespace Strabo.Core.SymbolRecognition
{
    public static class DBpedia
    {
    public class DbpediaInfo
        {
            public string uri;
            public double lng;
            public double lat;
            public string type;
        }
      



        public static List<DbpediaInfo> getDbpediaInfo(double leftX, double leftY, double rightX, double rightY)
        {
            List<DbpediaInfo> DbpediaArray = new List<DbpediaInfo>();
            #region

            // Dataset dataset = TDBFactory.createDataset(@"C:\Users\simakmo\Desktop\Jena");
            //Query query = QueryFactory.create("SELECT * WHERE { "
            //        + "?e <http://dbpedia.org/ontology/series> <http://dbpedia.org/resource/The_Sopranos>  ."
            //        + "?e <http://dbpedia.org/ontology/releaseDate> ?date ."
            //        + "?e <http://dbpedia.org/ontology/episodeNumber>  ?number  . "
            //        + "?e <http://dbpedia.org/ontology/seasonNumber>   ?season ."
            //        + " }" + "ORDER BY DESC(?date)");
            //QueryExecution qexec = QueryExecutionFactory.create(query, dataset);
            //ResultSet results = qexec.execSelect();

            //            Query query = QueryFactory.create(@"PREFIX p: <http://dbpedia.org/property/>
            //            PREFIX dbpedia: <http://dbpedia.org/resource/>
            //            PREFIX category: <http://dbpedia.org/resource/Category:>
            //            PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
            //            PREFIX skos: <http://www.w3.org/2004/02/skos/core#>
            //            PREFIX geo: <http://www.georss.org/georss/> 
            //            SELECT DISTINCT ?m ?n ?p ?d WHERE { ?m rdfs:label ?n. ?m skos:subject ?c. 
            //            ?c skos:broader category:Churches_in_Paris. ?m p:abstract ?d. ?m geo:point ?p }
            //            ");

            //   String query = "SELECT ?abstract" +
            //"WHERE {" +
            //"{" +
            //"<http://dbpedia.org/resource/Akbar> <http://dbpedia.org/ontology/abstract> ?abstract." +
            //"FILTER langMatches( lang(?abstract), 'en')" +
            //"}" +
            //"}";



            //    string  query = "Prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>" +
            //"Prefix ogc: <http://www.opengis.net/ont/geosparql#>" + "Prefix geom: <http://geovocab.org/geometry#>" + "Prefix lgdo: <http://linkedgeodata.org/ontology/>" +
            //"Select ?Restaurant From <http://linkedgeodata.org> {" + "?Restaurant a lgdo:Restaurant ;" + "rdfs:label ?RestaurantLabel ;" +
            //"geom:geometry [ ogc:asWKT ?RestaurantGeo ] ." +
            //  "}";

#endregion


            string query = "Prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>" +
                "Prefix ogc: <http://www.opengis.net/ont/geosparql#>" +
                "Prefix geom: <http://geovocab.org/geometry#>" +
                "Prefix lgdo: <http://linkedgeodata.org/ontology/>" +
                "PREFIX spatial: <http://jena.apache.org/spatial#>" +
                "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>" +

                "PREFIX owl: <http://www.w3.org/2002/07/owl#>" +
                "PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>" +
                "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>" +
                "PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>" +
                "PREFIX foaf: <http://xmlns.com/foaf/0.1/>" +
                "PREFIX dc: <http://purl.org/dc/elements/1.1/>" +
                "PREFIX : <http://dbpedia.org/resource/>" +
                "PREFIX dbpedia2: <http://dbpedia.org/property/>" +
                "PREFIX dbpedia: <http://dbpedia.org/>" +
                "PREFIX skos: <http://www.w3.org/2004/02/skos/core#>" +
                "PREFIX geo:<http://www.w3.org/2003/01/geo/wgs84_pos#>" +
                " PREFIX spatial: <http://jena.apache.org/spatial#>" + "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>" +
                "SELECT *  WHERE {" + " ?uri geo:lat ?lat ." + "?uri geo:long ?lon ." + " ?uri rdf:type ?thetype ." + "  FILTER ( (?lon>"+ rightX.ToString()+"   && ?lon <" + leftX.ToString()+") &&" +
                "(?lat>"+ leftY.ToString()+"  && ?lat <" +rightY.ToString()+")" + " && regex(?thetype,'^http://schema.org')" + " )" + "}";








#region test
            //"SELECT ?c ?geo " + "From <http://dbpedia.org/snorql/>" +
            //  "WHERE {" + "?m geo:geometry ?geo ." + " ?m a ?c ." + "FILTER (bif:st_within(?geo, bif:st_point (33.325604, 44.409851  ), 100))"
            // + "}";

            //QueryExecution qexec = QueryExecutionFactory.create(query, dataset);
            //String service = "http://linkedgeodata.org/";



            // now creating query object
            //Query queryString = QueryFactory.create(query);

            // initializing queryExecution factory with remote service.
            // **this actually was the main problem I couldn't figure out.**


            //string query = " PREFIX spatial: <http://jena.apache.org/spatial#>" + "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>" +
            // "SELECT ?placeName {" + "?place spatial:query (33.33 44.38  10 'km') ." + " ?place rdfs:label ?placeName" + "}";
#endregion

            QueryExecution qexec = QueryExecutionFactory.sparqlService("http://dbpedia.org/sparql", query);//"http://linkedopencommerce.com/sparql/", query);//"http://dbpedia.org/sparql", query);


            try
            {
                com.hp.hpl.jena.query.ResultSet results = qexec.execSelect();

                for (; results.hasNext(); )
                {
                    QuerySolution soln = results.nextSolution();
                    DbpediaInfo info = new DbpediaInfo();
                    //((com.hp.hpl.jena.sparql.resultset.XMLInputStAX.ResultSetStAX)(results)).binding
                    //                    java.util.Iterator test = soln.varNames();

                    // RDFNode x = soln.get("resource");     
                    // Get a result variable by name.
                    //Resource r = soln.getResource("VarR"); // Get a result variable - must be a resource
                    // Literal l = soln.getLiteral("VarL");   // Get a result variable - must be a literal
                    // Result processing is done here.


                    info.lat = double.Parse(soln.get("lat").toString().Split('^')[0]);
                    info.lng = double.Parse(soln.get("lon").toString().Split('^')[0]);
                    info.uri = soln.get("uri").toString();
                    info.type = soln.get("thetype").toString();
                    DbpediaArray.Add(info);

                }
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                qexec.close();
            }

            return DbpediaArray;
        }
    }
}
