# Backlog do TINS

## Ordem sugerida no playtest

1. Ajustar o balanceamento dentro da missão e a leitura da recarga do flash.
2. Decidir como trocar o Hub por um menu e encaixar o tutorial.
3. Criar os símbolos dos recursos.
4. Definir a progressão do mapa e alinhar a árvore de habilidades aos recursos disponíveis em cada etapa.

## Flash e interface de missão

- [x] Tornar a recarga do flash fácil de ler. A barra sobre o jogador mostra as cargas, o tempo restante e o aviso de recarga concluída. No Play Mode, a recarga normal levou cerca de 2 s e o acerto na marca reduziu para 1,57 s.
- [x] Priorizar a UI das luzes acima da cabeça do jogador. O HUD de dash está desativado na cena da missão.
- [x] Adicionar uma barra de progresso no topo da missão com marcadores para os elites. O marcador sinaliza a aproximação do limite, distingue quando o elite já pode surgir e muda após o surgimento.

## Balanceamento dentro da missão

- [x] Testar crescimento de **0,1 a 0,2 unidade de raio por alma entregue** e reduzir a chance inicial de os inimigos deixarem almas. Vharn começa com 80% de chance e cresce 0,15 por entrega. A coleta e a expansão foram verificadas em Play Mode.
- [x] Configurar **raio final de 20 a 30** na maior parte das missões, com mais recursos como recompensa para objetivos maiores. O catálogo carregado em Play Mode usa 20, 22, 24, 26 e 28 nos cinco planetas após Vharn.
- [x] Ajustar a vida dos inimigos. Vharn começa com o Pretinha, que cai com dois tiros da pistola inicial. Inimigos mais resistentes entram ao longo da missão, incluindo o Espinho Andante no primeiro marco de elite.
- [x] Balancear o raio, a frequência de surgimento e o objetivo final. Na verificação final de Vharn, a luz cresceu de 4 para 15,4 em 125 s de jogo, com 93 abates e 78 almas recolhidas. O intervalo caiu de 1,65 s para 0,85 s. A medição usou mira e deslocamento assistidos.
- [x] Avaliar melhorias de crescimento da luz e de chance de drop. Luz Nutrida aumenta o raio por alma em 8% por nível e Colheita de Almas aumenta a chance de drop em 3 pontos percentuais por nível. O teste confirmou crescimento de 0,174 por alma em Vharn com dois níveis de Luz Nutrida e um drop adicional com Colheita de Almas.

## Mapa, missões e economia

- [x] Dar ao jogador dois destinos após Vharn. Khal-Neth e Oth-Zai abrem caminhos separados e acesso cedo a Cristal Frio e Poeira Estelar. Nhalassar exige a purificação dos dois ramos.
- [x] Acender o Sol quando todos os planetas forem purificados. O mapa mantém o Sol apagado até o último planeta e atualiza a arte e o texto ao completar a campanha.
- [x] Em planetas purificados, oferecer **Proteger**. O objetivo é manter o centro de luz ativo por 75 s. O temporizador, o surgimento de elites e o pagamento da recompensa foram testados em Play Mode.
- [x] Extrair o jogador após a vitória. Depois da onda de purificação, a extração começa e pode ser concluída interagindo perto do centro. Após 7 s, o retorno à Base é automático. Os dois finais foram testados em Play Mode.
- [x] Espalhar caches de recursos nas áreas escuras. O primeiro cache fica na penumbra inicial e os demais ficam acessíveis conforme a luz cresce. A coleta automática foi testada em Vharn, e a posição dos caches foi verificada nos seis planetas. A quantidade de caches e o rendimento por cache são ajustáveis no `CampaignCatalog`.
- [x] Criar um símbolo para cada recurso, com silhueta e cor próprias. Os sete símbolos estão no atlas da interface e as almas dos inimigos usam a folha de 10 × 10.

## Árvore de habilidades

- [x] Atualizar e compartilhar a lista de habilidades. A [documentação da árvore](Arvore-de-Habilidades.md) lista os efeitos, os níveis e os custos em materiais da campanha.
- [x] Testar as três opções básicas custando só almas. Flash de Luz, Passos Livres e Vigor Solar têm preços crescentes até os níveis 9, 6 e 5, respectivamente.
- [x] Alinhar custos ao percurso **Éter → Cristal Frio → Fragmento Umbral**. A Pistola Astral exige almas e Éter, a Pistola de Rajada exige almas e Cristal Frio, e o segundo nível de Lâminas Guardiãs exige Fragmento Umbral.
- [x] Reembolsar o último nível pago de uma habilidade. O botão devolve o custo registrado de almas e materiais e protege os pré-requisitos de habilidades compradas.

## Menu, tutorial e ferramentas

- [x] Criar o menu inicial com atalhos para missões e habilidades. A Base continua acessível pela opção **Explorar nave**.
- [x] Criar um tutorial para a primeira missão. As instruções avançam com o movimento, o primeiro disparo, a coleta e a entrega de almas.
- [x] Simplificar o Inspector dos ScriptableObjects. O catálogo agrupa luz, inimigos, recompensas, caches e rotas. As rotas usam nomes de planetas e excluem dependências cíclicas. A habilidade usa um seletor de materiais e mostra os campos pertinentes ao efeito escolhido.


