using System;
using Xunit;
using api.Utils;

namespace tests
{
    public class TemplateConverterTest
    {
        [Fact]
        public void TestTemplateWithMultipleCitiesAndNeighborhoods()
        {
            string template = @"<corpo>
            {{for city in cities}}
            <cidade>
            <nome>{{city}}</nome>
            <populacao>{{population}}</populacao>
            <bairros>
            {{for neighborhood in city.neighborhoods}}
            <bairro>
            <nome>{{neighborhood}}</nome>
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
            <populacao>135678</populacao>
            </bairro>
            <bairro>
            <nome>Vila Isabel</nome>
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
            <populacao>357890</populacao>
            </bairro>
            <bairro>
            <nome>Centro</nome>
            <populacao>24567</populacao>
            </bairro>
            </bairros>
            </cidade>
            </corpo>";
            
            var converter = new TemplateConverter();
            var cities = converter.GetCitiesFromTemplate(template, input);

            Assert.Equal(2, cities.Length);
            Assert.Equal("Rio de Janeiro", cities[0].Name);
            Assert.Equal("Niterói", cities[1].Name);
            Assert.Equal(10345678, cities[0].Population);
            Assert.Equal(87654321, cities[1].Population);
            Assert.Equal(2, cities[0].Neighborhoods.Length);
            Assert.Equal(2, cities[1].Neighborhoods.Length);
            Assert.Equal("Tijuca", cities[0].Neighborhoods[0].Name);
            Assert.Equal("Vila Isabel", cities[0].Neighborhoods[1].Name);
            Assert.Equal("Icaraí", cities[1].Neighborhoods[0].Name);
            Assert.Equal("Centro", cities[1].Neighborhoods[1].Name);
            Assert.Equal(135678, cities[0].Neighborhoods[0].Population);
            Assert.Equal(246789, cities[0].Neighborhoods[1].Population);
            Assert.Equal(357890, cities[1].Neighborhoods[0].Population);
            Assert.Equal(24567, cities[1].Neighborhoods[1].Population);
        }
    }
}
