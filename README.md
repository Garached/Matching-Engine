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

Para rodar os testes automatizados, a partir da pasta raiz:

```bash
dotnet test
```

## Comandos disponíveis

| Comando | Descrição |
|---|---|
| `limit buy <preco> <qty>` | Cria uma ordem de compra limite |
| `limit sell <preco> <qty>` | Cria uma ordem de venda limite |
| `market buy <qty>` | Executa uma compra ao melhor preço disponível |
| `market sell <qty>` | Executa uma venda ao melhor preço disponível |
| `print book` | Exibe o livro de ordens e o spread atual |
| `cancel order <id>` | Cancela uma ordem pelo identificador (`1` ou `order_1`) |
| `modify <id> <preco> <qty>` | Altera preço e/ou quantidade de uma ordem (use `-` para manter um valor) |
| `peg bid buy <qty>` | Cria uma ordem de compra que segue o melhor preço de compra |
| `peg offer sell <qty>` | Cria uma ordem de venda que segue o melhor preço de venda |
| `exit` | Encerra o programa |

Preços usam ponto como separador decimal (ex: `9.98`), independente da configuração regional da máquina.

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

O livro de ordens utiliza dois heaps (um para compras e um para vendas), garantindo acesso ao melhor preço em O(1) amortizado e inserção em O(log N).

Um `Dictionary` complementa os heaps, permitindo buscar, cancelar e modificar ordens por Id em O(1). Toda ordem que deixa de existir, seja por cancelamento ou por execução total, é removida do dicionário.

**Preço de execução**

O trade é executado no preço da ordem passiva, ou seja, a que já estava no livro. A ordem agressora aceita esse preço, o que pode representar uma melhoria de preço para ela (price improvement).

**Limit orders que cruzariam**

Quando uma limit order entra com preço que já permitiria um trade (ex: limit buy a R$20 com sell disponível a R$15), o sistema executa o trade imediatamente, pois ignorar essa situação não faria sentido do ponto de vista financeiro.

**Cancelamento**

Como o `PriorityQueue` do C# não suporta remoção de elementos do meio, ordens canceladas têm sua quantidade zerada (lazy deletion). Todo acesso ao melhor preço passa pelos métodos `BestBid` e `BestAsk`, que descartam essas ordens do topo do heap antes de retornar. Cada ordem cancelada é descartada uma única vez, então o custo de limpeza é amortizado.

**Ordem de chegada (FIFO)**

Ordens com o mesmo preço são priorizadas pela ordem de chegada. Implementado usando o Id como segundo critério de prioridade no heap: Ids menores chegaram primeiro.

**Alteração de ordens**

Segue a lógica de cancel/replace usada no mercado: reduzir a quantidade altera a ordem no lugar e mantém a prioridade; alterar o preço ou aumentar a quantidade cancela a ordem e a recria no fim da fila, com um novo Id. Isso impede que uma ordem pequena "guarde lugar" na fila e depois seja aumentada.

**Ordens pegged**

Acompanham o melhor preço de compra (peg bid) ou de venda (peg offer). Quando o preço de referência muda, a ordem é reposicionada por cancel/replace, e o sistema informa o novo Id.

**Liquidez insuficiente**

Market orders que não conseguem ser totalmente preenchidas exibem um aviso informando o quanto foi executado.

**Validação de entrada**

Comandos com lado, preço ou quantidade inválidos são recusados com uma mensagem explicativa. Preço e quantidade precisam ser maiores que zero.

## Testes

O projeto `MatchingEngine.Tests` usa xUnit e cobre os cenários de cada correção listada abaixo. Como o `OrderBook` imprime os resultados no console, os testes redirecionam a saída para capturar e verificar o que seria impresso.

## Melhorias pós-avaliação

Após chegar à etapa final do processo seletivo, revisei o projeto e corrigi os seguintes pontos, cada um em um commit separado:

1. **Preço do trade:** o trade sempre saía no preço da ordem de venda, o que estava errado quando a venda era a ordem agressora. Agora usa o preço da ordem passiva.
2. **Peg e ordens canceladas:** o peg lia o topo do heap diretamente e podia copiar o preço de uma ordem cancelada. O acesso ao melhor preço foi centralizado em `BestBid` e `BestAsk`.
3. **Ordens executadas no dicionário:** ordens totalmente executadas saíam do heap mas continuavam no dicionário, podendo ser "canceladas" e ocupando memória.
4. **Modify:** alterar só o preço criava uma ordem com quantidade zero, e aumentar a quantidade mantinha a prioridade na fila. Reescrito seguindo cancel/replace.
5. **Validação e cultura:** entradas inválidas eram aceitas silenciosamente (ex: lado desconhecido virava venda), e em máquinas com configuração regional brasileira `9.98` era lido como `998`.
6. **Id do peg:** ordens pegged não informavam o Id na criação nem no reposicionamento, impossibilitando cancelá-las.
7. **Testes automatizados** cobrindo os cenários acima.

Nesta revisão, usei IA (Claude) como revisor de código para identificar bugs e discutir soluções. Cada correção foi implementada, testada e documentada por mim, e sei explicar o raciocínio de cada uma.

## Limitações conhecidas

- O `OrderBook` imprime os resultados em vez de retorná-los, o que acopla a lógica à interface de console e dificulta testes e reuso.
- Ordens canceladas permanecem no heap até chegarem ao topo, então o heap pode crescer além do número de ordens ativas.
- Como alterações de preço e reposicionamentos de peg usam cancel/replace, o Id da ordem muda nessas operações.
