# ⚙️ Video Download Consumer

Aplicação console responsável pelo consumo das mensagens da fila e processamento dos downloads de vídeos do YouTube.

## 📌 Sobre o projeto

O Consumer é responsável por receber as mensagens publicadas pela API através do RabbitMQ e executar o processamento do download utilizando o `yt-dlp`.

Após receber a solicitação:

1. O consumer consome a mensagem da fila
2. O serviço interpreta os dados enviados pela API
3. O `yt-dlp` executa o download do vídeo
4. O arquivo é salvo no diretório configurado

---

## 🚀 Tecnologias

- .NET
- RabbitMQ
- yt-dlp
- Background Services
- Hosted Services

---

## 📂 Estrutura do processamento

```text
API -> RabbitMQ -> Consumer -> yt-dlp -> Download do arquivo