FROM fluent/fluentd:v1.16-debian

# Instale dependências do sistema
USER root
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    libffi-dev \
    libssl-dev \
    libyaml-dev \
    zlib1g-dev \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

# Instale o plugin kafka
RUN gem install fluent-plugin-kafka --no-document

# Copie a configuração para o container
COPY fluent.conf /fluentd/etc/

# Exponha as portas padrão
EXPOSE 24224 24224/udp

# Altere para o usuário padrão do FluentD
USER fluent
