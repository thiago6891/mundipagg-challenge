# Desafio Mundipagg

## Instalação

### Requisitos

- .Net Core 2.0+
- Docker 18.03.1-ce+ (*Teoricamente, funciona na versão 17.09 ou superior. Porém, não testei.*)

### Instruções

Executar os seguintes comandos para clonar o repositório, ir até a pasta raiz do projeto, e buildar a imagem Docker.

    ~$ git clone https://github.com/thiago6891/mundipagg-challenge.git
    ~$ cd mundipagg-challenge/
    ~$ docker build -t mundipagg-web-api ./api

## Execução

Executar os seguintes comandos para fazer o deploy do stack.

    ~$ docker swarm init
    ~$ docker stack deploy -c docker-compose.yml mundipagg

- Caso o comando ***docker swarm init*** apresente erro, será necessário executá-lo com a opção ***--advertise-addr*** de acordo com as instruções que irão aparecer no terminal.

## Acessando a API

A API ficará hospedada em http://localhost:5000/ nos seguintes endpoints:

- **GET api/templates**: Retorna uma lista com todos os templates disponíveis.
- **POST api/templates**: Salva o template que deverá ser enviado no corpo do request.
- **DELETE api/templates**: Deleta o template que deverá ser enviado no corpo de request.
- **POST api/convert**: Converte para o formato padrão os dados que forem enviados no corpo do request.

*As regras de criação de um novo template serão explicadas abaixo.*

### Aplicação Web

Com o fim de facilitar os testes manuais da API. Criei uma aplicação web bem simples em http://localhost:5000/index.html

## Derrubando a Aplicação

Para derrubar a aplicação basta executar os seguintes comandos.

    ~$ docker stack rm mundipagg
    ~$ docker swarm leave --force

## Templates Persistidos

Os templates são armazenados com o Redis em um volume Docker com o nome *template-redis*.

Para deletar o BD, basta executar o comando:

    ~$ docker volume rm template-redis

## Testes Unitários

Para executar os testes unitários, execute o seguinte comando a partir do diretório raiz do projeto.

    ~$ dotnet test tests/tests.csproj

## Sistema de Template

A *Engine* de conversão de dados para o formato padrão foi baseada na idéia de *Reverse Templating*, ou seja, usar um template (como Razor, mustache, etc.) para extrair variáveis ao invés de adicioná-las.

Primeiramente, tentei buscar uma algo pronto. Porém, não encontrei nenhuma solução robusta. Provavelmente pelo fato desse problema (*Reverse Templating*) não ser trivial.

Procurei deixar a solução o mais simples possível para ser aplicada no contexto sugerido pelo desafio.

Um template pode seguir o seguinte formato de exemplo:

    <cities>
        {{for city in cities}}
        <city>
            <name>{{city.name}}</name>
            <population>{{city.population}}</population>
            <neighborhoods>
                {{for neighborhood in city.neighborhoods}}
                <neighborhood>
                    <name>{{neighborhood.name}}</name>
                    <population>{{neighborhood.population}}</population>
                </neighborhood>
                {{endfor}}
            <neighborhoods>
        </city>
        {{endfor}}
    </cities>

### Regras e Limitações

*... e algumas justificativas.*

1. Um template deve **necessariamente** conter um loop de cidades (*{{for city in cities}}*) com os *placeholders* de informações de cidade (*{{city.name}}* e *{{city.population}}*).

    - Acho que não faria muito sentido possuir dados de um censo demográfico sem as informações das cidades.

2. O loop de bairros (*{{for neighborhood in city.neighborhoods}}*) é **opcional**. Porém, caso esteja presente, os *placeholders* de informações de bairro (*{{neighborhood.name}}* e *{{neighborhood.population}}*)

    - Acho que poderia ser possível ter estados que forneçam dados somente à nível de cidade. Logo, optei por deixar os bairros como opcionais.

3. Como qualquer linguagem de programação (ou de template) os loops presentes devem ser fechados com o *{{endfor}}*. Além disso, as variáveis de cidades e bairros devem estar dentro de seus respectivos loops.

    Como exemplo, os templates abaixo seriam inválidos.

        (...)
        {{for city in cities}}
            (...)
            {{city.name}}
            (...)
        {{endfor}}
        (...)
        {{city.population}}
        (...)

    A variável *city* está fora de escopo.

        (...)
        {{for city in cities}}
            (...)
            {{city.name}}
            (...)
            {{city.population}}
            (...)

    O loop não é fechado com um *{{endfor}}*.

4. Não é possível ter mais de uma variável por linha. O exemplo abaixo seria inválido.

        (...)
        {{for city in cities}}
            (...)
            {{city.name}}(...){{city.population}}
            (...)
        {{endfor}}
        (...)

    - Devido ao modo como o sistema foi implementado (usando expressões regulares), as variáveis precisam estar em linhas diferentes para não causar ambiguidade.

5. Variáveis e loops que não possuem informações desejadas para gerar os dados de saída devem ser sinalidos usando-se a notação {{}}.

    Por exemplo:

        <body>
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
        </body>

    No exemplo acima temos um loop de regiões com um loop de cidades em cada região.

    Os nomes dados para essas variáveis adicionais não importa, contanto que não sejam os mesmos dados para as variáveis que estamos buscando.

    Note também que o loop adicional foi fechado com *{{endfor region}}* ao invés de *{{endfor}}*.

    Qualquer outro nome poderia ser utilizado para fechar o loop. Na prática, quaisquer informações entre {{}}, além das que buscamos, serão ignoradas.

6. Caracteres que podem ou não aparecer nos dados de entrada também devem estar entre {{}}. Por exemplo:

        {
            "cities":[
                {{for city in cities}}
                {
                    "neighborhoods":[
                        {{for neighborhood in city.neighborhoods}}
                        {
                            "name": "{{neighborhood.name}}",
                            "population": "{{neighborhood.population}}"
                        }{{,}}
                        {{endfor}}
                    ],
                    "name": "{{city.name}}",
                    "population": "{{city.population}}"
                }{{,}}
                {{endfor}}
            ]
        }

    No template acima, que contempla uma entrada no formato JSON, a vírgula antes dos *{{endfor}}* pode ou não estar presente nos dados de entrada.

    Ela estaria presente ao final das informação de cada cidade, com exceção do última. Por exemplo:

        {
            "cities":[
                {
                    "neighborhoods":[
                    ],
                    "name": "Rio de Janeiro",
                    "population": "1234"
                },
                {
                    "neighborhoods":[
                    ],
                    "name": "Niterói",
                    "population": "5678"
                }
            ]
        }

    No exemplo de entrada acima podemos ver a vírgula **presente** no final da entrada correspondente ao *Rio de Janeiro*, porém **ausente** após os dados de *Niterói*.

7. As identações usadas nos exemplos até aqui são somente para melhor visualização dos templates, e não são requisitos para o funcionamento do sistema.

    - Na prática, toda identação, seja com *tabs* ou espaços, e espaços múltiplos consecutivos são transformados em um único espaço. Optei por essa solução principalmente por dois motivos:

        1. Para evitar que templates duplicados fossem persistidos erroneamente no banco.

        2. Para evitar que dados de entrada deixassem de ser convertidos devido à digitação acidental de um espaço a mais.

8. Embora múltiplos espaços não sejam um problema, a presença de espaços e caracteres de quebra de linha devem ser respeitados. Por exemplo:

        "name": "Rio de Janeiro"
        "name":"Rio de Janeiro"

    Os dois exemplos acima **não** são considerados iguais pela engine.

        "name": "Rio de Janeiro"
        "name":         "Rio de Janeiro"

    Porém, os dois últimos exemplos **são considerados iguais**.

        "neighborhoods": []
        "neighborhoods": [
        ]

    Assim como esses dois também não são iguais. As quebras de linha devem ser respaitadas.

9. Devido à não trivialidade do problema, templates que apresentem ambiguidade causarão bugs. O que não vai acontecer (pelo menos assim eu espero) com formatos como XML e JSON. Porém, podem surgir em formatos genéricos.

        {{for city in cities}}
        {{city.name}}
        {{city.population}}
        {{endfor}}

    Esse é um exemplo de um template que seria considerado válido mas causaria um bug.

    Internamente, as expressões regulares dariam *match* com **qualquer** entrada de texto, independente se o formato de entrada é correto ou não.

    - Esse foi um problema que só me deparei hoje enquanto fecho o desafio. Não consegui pensar numa solução, mas também não quero estourar o prazo que estipulei.

## Considerações Finais

- A aplicação web, idealmente, deveria estar num projeto separado que seria levantado em seu próprio container.

    Porém, foi algo que fiz rapidamente, de última hora, e com um mínimo de design, pensando em facilitar os testes para alguém "não-técnico".

- A classe *Template*, responsável por converter os dados de entrada para o formato padrão, está bem complexa.

    Dado mais tempo, teria trabalhado para diminuir a complexidade dela, deixando o código mais légivel e fácil de dar manutenção.

- Além disso, os testes no arquivo *TemplateTest.cs* estão testando duas classes diferentes (*Template* e *TemplateService*), que eu comecei a refatorar para desacoplar, porém não terminei.

    Um trabalho de diminuição da complexidade da classe *Template* incluiria esse desacoplamento (e provavelmente a criação de novas classes e redistribuição de responsabilidades).

- A escolha do Redis como banco não teve nem um motivo técnico além do fato de suportar *Sets*, o que ajuda a evitar a duplicação de templates.

    *(Havia pensado em usar o MongoDB, mas preferi optar por algo que já me fosse familiar e dedicar a maior parte do tempo ao problema principal: Reverse Templating.)*