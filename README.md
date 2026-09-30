# Matching Engine

Matching engine simples para cruzamento de ordens de compra e venda de um único ativo, desenvolvida para o processo seletivo de estágio em Strats da Morgan Stanley.

## Como rodar

Pré-requisito: [.NET SDK](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/Garached/Matching-Engine.git
cd Matching-Engine/MatchingEngine
dotnet run
```

Testes (a partir da pasta raiz): `dotnet test`

## Comandos

| Comando | Descrição |
|---|---|
| `limit buy/sell <preco> <qty>` | Cria uma ordem limite |
| `market buy/sell <qty>` | Executa ao melhor preço disponível |
| `peg bid buy <qty>` | Compra que segue o melhor preço de compra |
| `peg offer sell <qty>` | Venda que segue o melhor preço de venda |
| `modify <id> <preco> <qty>` | Altera uma ordem (`-` mantém o valor) |
| `cancel order <id>` | Cancela uma ordem (`1` ou `order_1`) |
| `print book` | Exibe o livro e o spread |
| `exit` | Encerra o programa |

Preços usam ponto decimal (`9.98`), independente da configuração regional.

## Exemplo

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
```

## Decisões de design

- **Estruturas:** dois heaps (compra e venda) dão o melhor preço em O(1) amortizado e inserção em O(log N); um `Dictionary` dá acesso por Id em O(1).
- **Prioridade:** preço, depois ordem de chegada (Id menor chegou antes).
- **Preço do trade:** o da ordem passiva, a que já estava no livro.
- **Trades reportados por ordem executada:** uma ordem que cruza com várias ordens gera uma linha de trade para cada uma.
- **Limit que cruza:** executa na hora, como numa bolsa real.
- **Cancelamento:** lazy deletion, já que o `PriorityQueue` não remove do meio. `BestBid`/`BestAsk` descartam ordens canceladas do topo.
- **Modify:** cancel/replace. Reduzir quantidade mantém a prioridade; mudar preço ou aumentar quantidade vai para o fim da fila.
- **Peg:** reposicionado por cancel/replace quando a referência muda, informando o novo Id.

## Testes

xUnit, em `MatchingEngine.Tests`: `OrderBookTests` cobre o livro de ordens e `InputParserTests` cobre a validação de entrada.

## Melhorias pós-avaliação

Após chegar à etapa final do processo, revisei o projeto e corrigi, em commits separados:

1. Trade saía sempre no preço da venda, mesmo quando ela era a agressora.
2. Peg podia copiar o preço de uma ordem cancelada.
3. Ordens executadas continuavam no dicionário e podiam ser "canceladas".
4. Modify zerava a quantidade ao mudar só o preço, e aumentar quantidade não perdia prioridade.
5. Entradas inválidas eram aceitas, e em máquinas brasileiras `9.98` virava `998`.
6. Peg não informava o Id, então não podia ser cancelado.
7. Livro exibia ordens de mesmo preço fora da ordem de prioridade.
8. Testes automatizados para os cenários acima.

Nesta revisão, usei IA (Claude) para identificar bugs, sugerir as correções e escrever os testes automatizados. Apliquei e validei cada mudança em commits separados, e sei explicar o raciocínio de cada uma.

## Limitações conhecidas

- O `OrderBook` imprime em vez de retornar resultados, acoplando a lógica ao console.
- Ordens canceladas ocupam o heap até chegarem ao topo.
- Cancel/replace muda o Id da ordem.
- Um peg que fica sozinho no topo passa a referenciar a si mesmo e não acompanha a queda do melhor preço.
