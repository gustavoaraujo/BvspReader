# BvspReader
Um robô que simula compras e vendas na Bovespa

A ideia de um robô como esse se divide em algumas partes: 
1 - a coleta de dados em um site específico (no caso, o site do pregão da Bovespa).
2 - a análise desses dados, o que permite definir se uma compra/venda deve ou não ocorrer de acordo com um saldo pré-estabelecido.

Isso é o que o BvspReader faz. De 10 em 10 minutos, no horário comercial, ele entra no pregão e simula compras e vendas.
