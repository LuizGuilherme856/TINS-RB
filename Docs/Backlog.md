# Backlog do TINS

## Ordem sugerida no playtest

1. Ajustar o balanceamento dentro da missão e a leitura da recarga do flash.
2. Decidir como trocar o Hub por um menu e encaixar o tutorial.
3. Criar os símbolos dos recursos.
4. Definir a progressão do mapa e alinhar a árvore de habilidades aos recursos disponíveis em cada etapa.

## Flash e interface de missão

- [x] Tornar a recarga do flash fácil de ler. A barra sobre o jogador mostra as cargas, o tempo restante e o aviso de recarga concluída. No Play Mode, a recarga normal levou cerca de 2 s e o acerto na marca reduziu para 1,57 s.
- [x] Priorizar a UI das luzes acima da cabeça do jogador. O HUD de dash está desativado na cena da missão.
- [x] Mostrar na barra de purificação os marcos de ondas comuns e de elites. Ao liberar uma onda, o HUD anuncia seu setor de entrada e mostra a contagem regressiva.

## Balanceamento dentro da missão

- [x] Testar crescimento de **0,1 a 0,2 unidade de raio por alma entregue** e reduzir a chance inicial de os inimigos deixarem almas. Velária começa com 80% de chance e cresce 0,15 por entrega. A coleta e a expansão foram verificadas em Play Mode.
- [x] Configurar **raio final de 20 a 30** nos cinco destinos após Velária. Nivora e Marévia têm metas de 20 e 21. Os demais têm metas de 22, 24 e 26. A recompensa segue a raridade de cada material.
- [x] Usar o Pretinha como inimigo comum de Velária, a Massa de Olhos como elite e o Rastejante do Bosque como chefe da onda final. O Pretinha se move mais rápido e a campanha omite o cristal antigo. Os seis planetas usam o elenco da campanha.
- [x] Aumentar a frequência e o limite de inimigos conforme a purificação cresce. O Play Mode confirmou a onda comum, a onda de elite e a exigência de derrotar o chefe de Velária para iniciar a extração.
- [x] Fazer as almas orbitarem o jogador a 24° por segundo na primeira posição. Uma entrega empurra os inimigos próximos para longe da torre.
- [x] Remover os 42 colisores de cenário em `03_ENVIRONMENT`. A derrota faz a luz piscar, desacelera a missão e escurece a tela antes de mostrar o resultado.
- [x] Avaliar melhorias de crescimento da luz e de chance de drop. Luz Nutrida aumenta o raio por alma em 8% por nível e Colheita de Almas aumenta a chance de drop em 3 pontos percentuais por nível. O teste confirmou crescimento de 0,174 por alma em Velária com dois níveis de Luz Nutrida e um drop adicional com Colheita de Almas.

## Mapa, missões e economia

- [x] Abrir os vizinhos **Nivora** e **Marévia** após purificar Velária. Eles dão acesso a Cristal Frio e Granalita. Nivora leva a Asterim, Marévia leva a Dorsália e os dois ramos liberam Varkora.
- [x] Acender o Sol quando todos os planetas forem purificados. O mapa mantém o Sol apagado até o último planeta e atualiza a arte e o texto ao completar a campanha.
- [x] Usar **Purificação** em todas as expedições, inclusive ao revisitar um planeta purificado. O raio de luz cresce com as almas entregues ao núcleo e libera os ataques. Após cada onda, os spawns comuns desaceleram por 6 s. Cada planeta define os marcos e o tamanho dos grupos no asset **Mission Enemies**.
- [x] Extrair o jogador após a vitória. Depois da onda de purificação, a extração começa e pode ser concluída interagindo perto do centro. Após 7 s, o retorno à Base é automático. Os dois finais foram testados em Play Mode.
- [x] Espalhar caches de recursos na penumbra. Ao se aproximar, pressione **F** para abrir o teste de precisão e pressione **F** de novo na marca para obter o material. A quantidade de caches e o rendimento por cache são ajustáveis no `CampaignCatalog`.
- [x] Criar um símbolo para cada recurso, com silhueta e cor próprias. Os sete símbolos estão no atlas da interface e as almas dos inimigos usam a folha de 10 × 10.

## Árvore de habilidades

- [x] Atualizar e compartilhar a lista de habilidades. A [documentação da árvore](Arvore-de-Habilidades.md) lista os efeitos, os níveis e os custos em materiais da campanha.
- [x] Testar as três opções básicas custando só almas. Flash de Luz, Passos Livres e Vigor Solar têm preços crescentes até os níveis 9, 6 e 5, respectivamente.
- [x] Alinhar custos ao percurso **Éter → Cristal Frio → Fragmento Umbral**. A Pistola Astral exige almas e Éter, a Pistola de Rajada exige almas e Cristal Frio, e o segundo nível de Lâminas Guardiãs exige Fragmento Umbral.
- [x] Usar a Granalita de Marévia no Mamaco Isca. A compra custa 65 almas e 2 Granalitas.
- [x] Reembolsar o último nível pago de uma habilidade. O botão devolve o custo registrado de almas e materiais e protege os pré-requisitos de habilidades compradas.

## Menu, tutorial e ferramentas

- [x] Criar o menu inicial com atalhos para missões e habilidades. A Base continua acessível pela opção **Explorar nave**.
- [x] Criar um tutorial para a primeira missão. As instruções avançam com o movimento, o primeiro disparo, a coleta e a entrega de almas.
- [x] Simplificar o Inspector dos ScriptableObjects. O catálogo agrupa luz, inimigos, recompensas, caches e rotas. As rotas usam nomes de planetas e excluem dependências cíclicas. A habilidade usa um seletor de materiais e mostra os campos pertinentes ao efeito escolhido.


