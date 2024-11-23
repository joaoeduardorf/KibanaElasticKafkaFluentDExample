# Observabilidade com Logs - Aplicação .NET 8

Este projeto demonstra como configurar um pipeline de observabilidade para uma aplicação **.NET 8**, utilizando **Serilog** para logs estruturados em JSON e uma cadeia de ferramentas de processamento de logs que inclui **FluentD**, **Kafka**, **Logstash**, **Elasticsearch** e **Kibana**. A aplicação consiste em uma API básica com endpoints CRUD e um endpoint de verificação de saúde.

---

## Funcionalidades

- **API em .NET 8**:
  - Endpoints CRUD (`GET`, `POST`, `PATCH`, `PUT`, `DELETE`).
  - Middleware para registrar logs detalhados de requisição e resposta.
  - Endpoint de verificação de saúde.
- **Pipeline de Logs**:
  - Logs gerados pela API são capturados pelo **FluentD** e enviados para o **Kafka**.
  - O **FluentD** consome mensagens do Kafka no tópico `application-logs`.
  - Mensagens são processadas pelo FluentD e podem ser encaminhadas para diferentes destinos, como Elasticsearch.
  - Logs armazenados no **Elasticsearch** e visualizados no **Kibana**.

---

## Pré-requisitos

- **Docker** e **Docker Compose** instalados.
- **SDK do .NET 8** para compilar a aplicação.
- Certificados gerados para o Elasticsearch, caso esteja usando TLS.

---

## Configuração do Projeto

### 1. Clone o Repositório

```bash
git clone https://github.com/seu-usuario/observability-dotnet8.git
cd observability-dotnet8
````

### 2. Configure o FluentD

Certifique-se de que o arquivo `fluentd/fluent.conf` está configurado corretamente para consumir o tópico Kafka `application-logs`:

```plaintext
<source>
  @type kafka_group
  brokers kafka:9092
  topics application-logs
  group_id fluentd-consumer-group
  format json
</source>

<match **>
  @type stdout
</match>
```

### 3. Atualize o Docker Compose

O arquivo `docker-compose.yml` deve incluir todos os serviços necessários:

````yaml
fluentd:
  build:
    context: ./fluentd
  ports:
    - "24224:24224"
    - "24224:24224/udp"
  depends_on:
    - kafka
  environment:
    - FLUENTD_CONF=fluent.conf
  volumes:
    - ./fluentd/fluent.conf:/fluentd/etc/fluent.conf
````

### 4. Construa e Inicie os Serviços

Execute o seguinte comando para subir todos os serviços:

````bash
docker-compose up --build
````

---

## Teste da Aplicação

### 1. Acesse a API

- URL Base: `http://localhost:5000`
- Endpoints:
  - `GET /api/health`: Verifica a saúde da aplicação.
  - `POST /api/sample`: Cria um exemplo.
  - Outros: `GET`, `PATCH`, `PUT`, `DELETE`.

---

### 2. Produza uma Mensagem no Kafka

Envie uma mensagem de teste para o tópico `application-logs`:

````bash
docker exec -it kafka kafka-console-producer --broker-list kafka:9092 --topic application-logs
````

Exemplo de mensagem:

````json
{"message": "Hello from Kafka"}
````

### 3. Verifique os Logs no FluentD

Confira os logs no FluentD para verificar o consumo das mensagens do Kafka:

````bash
docker logs fluentd
````

---

### 4. Visualize os Logs no Kibana

- Acesse: [http://localhost:5601](http://localhost:5601)
- Faça login:
  - **Usuário**: `elastic`
  - **Senha**: configurada no `docker-compose.yml`.
- Navegue até **Discover** e selecione o índice de logs `application-logs-*`.

---

## Arquitetura do Pipeline

1. **FluentD**:
   - Consome logs do Kafka no tópico `application-logs`.
   - Encaminha os logs para a saída padrão ou Elasticsearch.
2. **Kafka**:
   - Broker de mensagens que gerencia os logs enviados pela aplicação.
3. **Elasticsearch**:
   - Armazena logs estruturados para buscas e análises.
4. **Kibana**:
   - Visualiza os logs armazenados no Elasticsearch.

---

## Estrutura do Projeto

````plaintext
.
├── app/                      # Aplicação em .NET 8
│   ├── Controllers/          # Controladores da API
│   ├── Middleware/           # Middleware para logging
│   ├── Program.cs            # Ponto de entrada
│   └── app.csproj            # Arquivo do projeto .NET
├── fluentd/
│   ├── Dockerfile            # Dockerfile personalizado para o FluentD
│   ├── fluent.conf           # Configuração do FluentD
├── docker-compose.yml        # Configuração do Docker Compose
└── README.md                 # Documentação
````

---

## Personalização

- Modifique o arquivo `fluentd/fluent.conf` para ajustar o roteamento dos logs.
- Adicione filtros no FluentD ou no Logstash para enriquecer as mensagens antes de armazená-las.
- Atualize o `docker-compose.yml` conforme necessário, incluindo variáveis de ambiente para produção.

---

## Ferramentas Utilizadas

- **.NET 8**: Framework da aplicação API.
- **Serilog**: Geração de logs estruturados.
- **FluentD**: Coletor e processador de logs.
- **Kafka**: Broker de mensagens.
- **Logstash**: Processador de mensagens (opcional).
- **Elasticsearch**: Armazenamento de logs.
- **Kibana**: Interface de visualização e análise.

---

## Licença

Este projeto está licenciado sob a [MIT License](LICENSE).

---

## Contato

Para dúvidas ou sugestões, entre em contato: [seu-email@dominio.com](mailto:seu-email@dominio.com).
