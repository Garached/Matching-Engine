# Matching Engine
 
Implementação de uma matching engine simples para cruzamento de ordens de compra e venda de um único ativo. 

Processo seletivo Morgan Stanley - área de Strats.
 
## Como rodar
 
Pré-requisito: [.NET SDK](https://dotnet.microsoft.com/download) instalado.
 
```bash
git clone https://github.com/Garached/Matching-Engine.git
cd Matching-Engine/MatchingEngine
dotnet run
```

## Comandos disponíveis

| Comando | Descrição |
|---|---|
| `limit buy <preco> <qty>` | Cria uma ordem de compra limite |
| `limit sell <preco> <qty>` | Cria uma ordem de venda limite |
| `market buy <qty>` | Executa uma compra ao melhor preço disponível |
| `market sell <qty>` | Executa uma venda ao melhor preço disponível |
| `print book` | Exibe o livro de ordens e o spread atual |
| `cancel order <id>` | Cancela uma ordem pelo identificador |
| `modify <id> <preco> <qty>` | Altera preço e/ou quantidade de uma ordem |
| `peg bid buy <qty>` | Cria uma ordem que segue o melhor preço de compra |
| `peg offer sell <qty>` | Cria uma ordem que segue o melhor preço de venda |
| `exit` | Encerra o programa |

## Exemplo de uso
 
```
>>> limit buy 10 100
Order created: buy 100 @ 10 order_1
>>> limit sell 20 100
Order created: sell 100 @ 20 order_2
>>> limit sell 20 200
Order created: sell 200 @ 20 order_3
>>> market buy 150
Trade, price: 20, qty: 100
Trade, price: 20, qty: 50
>>> print book
Ordens de Compra
100 @ 10
Ordens de Venda
150 @ 20
Spread: 10
>>> exit
Programa encerrado
```
 
---
 
## Especificações do projeto
 
**Estruturas de dados**
 
O livro de ordens utiliza dois heaps (um para compras e um para vendas), garantindo acesso ao melhor preço em O(1) e inserção em O(log N).
 
Um `Dictionary` complementa os heaps, permitindo buscar, cancelar e modificar ordens por Id em O(1).
 
**Limit orders que cruzariam**
 
Quando uma limit order entra com preço que já permitiria um trade (ex: limit buy a R$20 com sell disponível a R$15), o sistema executa o trade imediatamente, pois ignorar essa situação não faria sentido do ponto de vista financeiro.
 
**Cancelamento**
 
Como o `PriorityQueue` do C# não suporta remoção de elementos do meio, ordens canceladas têm sua quantidade zerada (lazy deletion). O Match descarta essas ordens quando aparecem no topo do heap.
 
**Ordem de chegada (FIFO)**
 
Ordens com o mesmo preço são priorizadas pela ordem de chegada. 
Implementado usando o Id como segundo critério de prioridade no heap: Ids menores chegaram primeiro.

**Liquidez insuficiente**

Market orders que não conseguem ser totalmente preenchidas exibem um aviso informando o quanto foi executado.
