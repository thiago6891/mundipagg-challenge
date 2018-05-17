using api.Interfaces;

namespace api.Services
{
    public class StoreService : IStoreService
    {
        public string[] GetTemplates()
        {
            var dummyTemplates = new string[]
            {
                @"
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
                ",
                @"<body>
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
                </body>",
                @"<corpo>
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
                </corpo>"
            };
            
            return dummyTemplates;
        }
    }
}