using System;
using Xunit;
using api.Utils;

namespace tests
{
    public class TemplateTest
    {
        [Fact]
        public void TestOptionalCommaInJSONLikeTemplate()
        {
            var templateStr = @"
            {
                ""cities"":[
                    {{for city in cities}}
                    {
                        ""name"": ""{{city.name}}"",
                        ""population"": ""{{city.population}}"",
                        ""neighborhoods"":[
                            {{for neighborhood in city.neighborhoods}}
                            {
                                ""name"": ""{{neighborhood.name}}"",
                                ""population"": ""{{neighborhood.population}}""
                            }
                            {{endfor}}
                        ]
                    }{{,}}
                    {{endfor}}
                ]
            }
            ";
            var input = @"
            {
                ""cities"":[
                    {
                        ""name"": ""Rio Branco"",
                        ""population"": ""576589"",
                        ""neighborhoods"":[
                            {
                                ""name"": ""Habitasa"",
                                ""population"": ""7503""
                            }
                        ]
                    }
                ]
            }
            ";

            var template = new Template(templateStr);
            var cities = template.ExtractCities(input);

            Assert.Single(cities);
            Assert.Equal("Rio Branco", cities[0].Name);
            Assert.Equal((UInt32)576589, cities[0].Population.Value);
            Assert.Single(cities[0].Neighborhoods);
            Assert.Equal("Habitasa", cities[0].Neighborhoods[0].Name);
            Assert.Equal((UInt32)7503, cities[0].Neighborhoods[0].Population);
        }

        [Fact]
        public void TestDataBeforeAndAfterNeighborhoods()
        {
            var templateStr = @"
            {
                ""cities"":[
                    {{for city in cities}}
                    {
                        ""name"": ""{{city.name}}"",
                        ""neighborhoods"":[
                            {{for neighborhood in city.neighborhoods}}
                            {
                                ""name"": ""{{neighborhood.name}}"",
                                ""population"": ""{{neighborhood.population}}""
                            }
                            {{endfor}}
                        ],
                        ""population"": ""{{city.population}}""
                    }{{,}}
                    {{endfor}}
                ]
            }
            ";
            var input = @"
            {
                ""cities"":[
                    {
                        ""name"": ""Rio Branco"",
                        ""neighborhoods"":[
                            {
                                ""name"": ""Habitasa"",
                                ""population"": ""7503""
                            }
                        ],
                        ""population"": ""576589""
                    }
                ]
            }
            ";

            var template = new Template(templateStr);
            var cities = template.ExtractCities(input);

            Assert.Single(cities);
            Assert.Equal("Rio Branco", cities[0].Name);
            Assert.Equal((UInt32)576589, cities[0].Population.Value);
            Assert.Single(cities[0].Neighborhoods);
            Assert.Equal("Habitasa", cities[0].Neighborhoods[0].Name);
            Assert.Equal((UInt32)7503, cities[0].Neighborhoods[0].Population);
        }

        [Fact]
        public void TestDataAfterNeighborhoodsOnly()
        {
            var templateStr = @"
            {
                ""cities"":[
                    {{for city in cities}}
                    {
                        ""neighborhoods"":[
                            {{for neighborhood in city.neighborhoods}}
                            {
                                ""name"": ""{{neighborhood.name}}"",
                                ""population"": ""{{neighborhood.population}}""
                            }
                            {{endfor}}
                        ],
                        ""name"": ""{{city.name}}"",
                        ""population"": ""{{city.population}}""
                    }{{,}}
                    {{endfor}}
                ]
            }
            ";
            var input = @"
            {
                ""cities"":[
                    {
                        ""neighborhoods"":[
                            {
                                ""name"": ""Habitasa"",
                                ""population"": ""7503""
                            }
                        ],
                        ""name"": ""Rio Branco"",
                        ""population"": ""576589""
                    }
                ]
            }
            ";

            var template = new Template(templateStr);
            var cities = template.ExtractCities(input);

            Assert.Single(cities);
            Assert.Equal("Rio Branco", cities[0].Name);
            Assert.Equal((UInt32)576589, cities[0].Population.Value);
            Assert.Single(cities[0].Neighborhoods);
            Assert.Equal("Habitasa", cities[0].Neighborhoods[0].Name);
            Assert.Equal((UInt32)7503, cities[0].Neighborhoods[0].Population);
        }

        [Fact]
        public void TestSingleCityAndNeighborhood()
        {
            string templateStr = @"<body>
            <region>
            <name>{{region}}</name>
            <cities>
            {{for city in cities}}
            <city>
            <name>{{city.name}}</name>
            <population>{{city.population}}</population>
            <neighborhoods>
            {{for neighborhood in city.neighborhoods}}
            <neighborhood>
            <name>{{neighborhood.name}}</name>
            <zone>{{neighborhood.zone}}</zone>
            <population>{{neighborhood.population}}</population>
            </neighborhood>
            {{endfor}}
            </neighborhoods>
            </city>
            {{endfor}}
            </cities>
            </region>
            </body>";
            string input = @"<body>
            <region>
            <name>Triangulo Mineiro</name>
            <cities>
            <city>
            <name>Uberlandia</name>
            <population>700001</population>
            <neighborhoods>
            <neighborhood>
            <name>Santa Monica</name>
            <zone>Zona Leste</zone>
            <population>13012</population>
            </neighborhood>
            </neighborhoods>
            </city>
            </cities>
            </region>
            </body>";

            var template = new Template(templateStr);
            var cities = template.ExtractCities(input);

            Assert.Single(cities);
            Assert.Equal("Uberlandia", cities[0].Name);
            Assert.Equal((UInt32)700001, cities[0].Population.Value);
            Assert.Single(cities[0].Neighborhoods);
            Assert.Equal("Santa Monica", cities[0].Neighborhoods[0].Name);
            Assert.Equal((UInt32)13012, cities[0].Neighborhoods[0].Population);
        }

        [Fact]
        public void TestOptionalLoopAndAbsentNeighborhoods()
        {
            string templateStr = @"<body>
            {{for region in regions}}
            <region>
            <name>{{region}}</name>
            <cities>
            {{for city in cities}}
            <city>
            <name>{{city.name}}</name>
            <population>{{city.population}}</population>
            <neighborhoods>
            {{for neighborhood in city.neighborhoods}}
            <neighborhood>
            <name>{{neighborhood.name}}</name>
            <zone>{{neighborhood.zone}}</zone>
            <population>{{neighborhood.population}}</population>
            </neighborhood>
            {{endfor}}
            </neighborhoods>
            </city>
            {{endfor}}
            </cities>
            </region>
            {{endfor region}}
            </body>";
            string input = @"<body>
            <region>
            <name>Triangulo Mineiro</name>
            <cities>
            <city>
            <name>Uberlandia</name>
            <population>700001</population>
            <neighborhoods>
            <neighborhood>
            <name>Santa Monica</name>
            <zone>Zona Leste</zone>
            <population>13012</population>
            </neighborhood>
            </neighborhoods>
            </city>
            </cities>
            </region>
            <region>
            <name>Zona da Mata</name>
            <cities>
            <city>
            <name>Juiz de Fora</name>
            <population>700002</population>
            <neighborhoods>
            </neighborhoods>
            </city>
            </cities>
            </region>
            </body>";

            var template = new Template(templateStr);
            var cities = template.ExtractCities(input);

            Assert.Equal(2, cities.Length);
            Assert.Equal("Uberlandia", cities[0].Name);
            Assert.Equal("Juiz de Fora", cities[1].Name);
            Assert.Equal((UInt32)700001, cities[0].Population.Value);
            Assert.Equal((UInt32)700002, cities[1].Population.Value);
            Assert.Single(cities[0].Neighborhoods);
            Assert.Empty(cities[1].Neighborhoods);
            Assert.Equal("Santa Monica", cities[0].Neighborhoods[0].Name);
            Assert.Equal((UInt32)13012, cities[0].Neighborhoods[0].Population);
        }

        [Fact]
        public void TestTemplateWithMultipleCitiesAndNeighborhoods()
        {
            string templateStr = @"<corpo>
            {{for city in cities}}
            <cidade>
            <nome>{{city.name}}</nome>
            <populacao>{{city.population}}</populacao>
            <bairros>
            {{for neighborhood in city.neighborhoods}}
            <bairro>
            <nome>{{neighborhood.name}}</nome>
            <regiao>{{region}}</regiao>
            <populacao>{{neighborhood.population}}</populacao>
            </bairro>
            {{endfor}}
            </bairros>
            </cidade>
            {{endfor}}
            </corpo>";
            string input = @"<corpo>
            <cidade>
            <nome>Rio de Janeiro</nome>
            <populacao>10345678</populacao>
            <bairros>
            <bairro>
            <nome>Tijuca</nome>
            <regiao>Zona Norte</regiao>
            <populacao>135678</populacao>
            </bairro>
            <bairro>
            <nome>Vila Isabel</nome>
            <regiao>Zona Norte</regiao>
            <populacao>246789</populacao>
            </bairro>
            </bairros>
            </cidade>
            <cidade>
            <nome>Niterói</nome>
            <populacao>87654321</populacao>
            <bairros>
            <bairro>
            <nome>Icaraí</nome>
            <regiao></regiao>
            <populacao>357890</populacao>
            </bairro>
            <bairro>
            <nome>Centro</nome>
            <regiao></regiao>
            <populacao>24567</populacao>
            </bairro>
            </bairros>
            </cidade>
            </corpo>";
            
            var template = new Template(templateStr);
            var cities = template.ExtractCities(input);

            Assert.Equal(2, cities.Length);
            Assert.Equal("Rio de Janeiro", cities[0].Name);
            Assert.Equal("Niterói", cities[1].Name);
            Assert.Equal((UInt32)10345678, cities[0].Population.Value);
            Assert.Equal((UInt32)87654321, cities[1].Population.Value);
            Assert.Equal(2, cities[0].Neighborhoods.Length);
            Assert.Equal(2, cities[1].Neighborhoods.Length);
            Assert.Equal("Tijuca", cities[0].Neighborhoods[0].Name);
            Assert.Equal("Vila Isabel", cities[0].Neighborhoods[1].Name);
            Assert.Equal("Icaraí", cities[1].Neighborhoods[0].Name);
            Assert.Equal("Centro", cities[1].Neighborhoods[1].Name);
            Assert.Equal((UInt32)135678, cities[0].Neighborhoods[0].Population);
            Assert.Equal((UInt32)246789, cities[0].Neighborhoods[1].Population);
            Assert.Equal((UInt32)357890, cities[1].Neighborhoods[0].Population);
            Assert.Equal((UInt32)24567, cities[1].Neighborhoods[1].Population);
        }
    }
}
