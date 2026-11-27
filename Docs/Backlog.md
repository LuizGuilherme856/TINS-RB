# Backlog do TINS

## Ordem sugerida no playtest

1. Ajustar o balanceamento dentro da missão e a leitura da recarga do flash.
2. Decidir como trocar o Hub por um menu e encaixar o tutorial.
3. Criar os símbolos dos recursos.
4. Definir a progressão do mapa e alinhar a árvore de habilidades aos recursos disponíveis em cada etapa.

## Flash e interface de missão

- [ ] Tornar a recarga do flash fácil de ler. Mostrar cargas disponíveis, recarga em andamento e quando o flash volta a ficar pronto. Testar um indicador maior ou mais central e avaliar a duração da recarga no playtest.
- [ ] Priorizar a UI das luzes acima da cabeça do jogador. Deixar a UI de dash para depois, quando o dash passar a ser necessário.
- [ ] Adicionar uma barra de progresso no topo da missão com marcadores que antecipem a chegada dos elites.

## Balanceamento dentro da missão

- [ ] Testar crescimento de **0,1 a 0,2 unidade de raio por alma entregue** e reduzir a chance inicial de os inimigos deixarem almas. Considerar melhorias que aumentem o crescimento e a chance de drop.
- [ ] Testar **raio final de 20 a 30** na maior parte das missões, com mais recursos como recompensa para objetivos maiores.
- [ ] Ajustar a vida dos inimigos. Trazer o Pretinha de volta como inimigo básico e introduzir inimigos mais resistentes ao longo da missão.
- [ ] Balancear juntos o raio atual da área iluminada, a frequência de spawn e o raio final do objetivo. Medir o resultado com os novos valores de drop, vida e crescimento por alma.

## Mapa, missões e economia

- [ ] Dar ao jogador pelo menos dois destinos após o planeta inicial. Se o mapa continuar em grafo, testar rotas de expansão ramificadas. Entre as ideias discutidas estão um planeta inicial no centro com acesso cedo a dois recursos e o uso de pop-ups de missão.
- [ ] Acender o Sol quando todos os planetas forem purificados.
- [ ] Em planetas já purificados, chamar a nova missão de **Proteger** ou **Defender**. O objetivo é manter o planeta purificado.
- [ ] Adicionar um evento de extração. Definir como ele é acionado e como termina.
- [x] Espalhar caches de recursos nas áreas escuras, dando um motivo para sair da área iluminada. A coleta é automática por proximidade. O primeiro cache fica na penumbra inicial e os demais ficam acessíveis conforme a luz cresce. A quantidade de caches e o rendimento por cache são ajustáveis por planeta no `CampaignCatalog`.
- [ ] Criar um símbolo para cada recurso, com silhueta e cor próprias.

## Árvore de habilidades

- [ ] Atualizar e compartilhar a lista de habilidades. Usar os assets da campanha e a [documentação da árvore](Arvore-de-Habilidades.md) como referência.
- [ ] Testar as três opções básicas custando só almas e avaliar se elas podem ter progressão contínua.
- [ ] Planejar os próximos custos a partir das rotas iniciais. Usar como referência a ordem **Éter → Cristal Frio → Fragmento Umbral** e testar custos compostos, como **almas + Éter** para desbloquear uma arma.
- [ ] Revisar o reembolso de habilidades e avaliar a devolução individual ou a desativação temporária de uma habilidade já comprada. A árvore já oferece um reset geral.

## Menu, tutorial e ferramentas

- [ ] Criar um menu do jogo
- [ ] Criar um tutorial para a primeira missão e encaixá-lo no fluxo inicial.
- [ ] Rever o autocomplete dos campos dos ScriptableObjects no Inspector. Identificar quais campos atrapalham a edição e como simplificá-los.


