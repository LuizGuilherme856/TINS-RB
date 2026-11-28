# Árvore de habilidades do TINS

A árvore nova usa assets `CampaignSkill` como fonte dos dados e os nós permanentes da cena `CampaignBase` como apresentação. As compras e recursos continuam no `CampaignProfile`; o sistema antigo de XP da árvore do Raining Blood não participa da progressão desta árvore.

## Adicionar uma habilidade

1. Crie um asset em **Assets > Create > TINS > Campaign > Skill**.
2. Preencha o `id` com um valor estável, como `flash-area-2`. O nome e a posição podem mudar; evite mudar o ID depois que existirem saves.
3. Configure nome, descrição, ícone, nível máximo, custo inicial, custo de desbloqueio e crescimento. Para custos específicos, preencha `soulCostsByRank` e `resourceCostsByRank`; cada posição do array representa um nível pago.
4. Escolha um `effectType` já suportado e ajuste os valores por nível, o nível em que o efeito começa e, se necessário, o segundo valor. Para armas, preencha também `unlockedWeaponResourcePath`.
5. Posicione o nó em `editorPosition` e configure os pré-requisitos de compra e revelação, incluindo o nível mínimo e a regra de exigir todos ou qualquer um.
6. Selecione o asset e use **Tools > There Is No Sun > Add Selected Skill Asset To Tree**. Isso registra a habilidade no `CampaignCatalog`, cria o nó permanente e desenha as conexões na cena.

Também é possível registrar o asset diretamente no array `skills` do `CampaignCatalog` e executar **Tools > There Is No Sun > Build Progressive Skill Tree**.

## Ajustar a posição visual

Os nós ficam em `CampaignBase > Campaign Skill Tree > Campaign Skill Tree Viewport > Campaign Skill Tree Content`. Arraste um nó na cena ou ajuste sua posição no `RectTransform`; no Inspector do nó, clique **Aplicar posição do nó ao asset** para persistir a posição em `editorPosition`. Para fazer o inverso, use **Aplicar posição do asset à cena**.

As conexões são derivadas dos pré-requisitos. Elas acompanham o nó durante a edição. No jogo, a estação abre a árvore em tela cheia. Arraste com o botão esquerdo para mover o mapa e use a roda para ajustar o zoom em torno do cursor. Os botões **+**, **−** e **Centralizar** também controlam a visão. Clique em um nó para ver seus detalhes e o custo no painel lateral.

## Simular a progressão

Abra **Tools > There Is No Sun > Preview Skill Tree Progression**. A janela usa as mesmas regras de compra e revelação que o jogo. Os valores da simulação ficam na memória e não são gravados no save do jogador.

## Efeitos disponíveis

Os efeitos existentes são aplicados a partir dos níveis comprados, sem acumular novamente ao reabrir menus ou trocar de cena: duração do Flash, ângulo e alcance do cone, cargas máximas, tempo de recarga, velocidade, vida e desbloqueio de arma. Para adicionar uma nova categoria de efeito, inclua um valor em `CampaignSkillEffectType` e conecte seu cálculo ao aplicador de efeitos da missão; compra, custos, revelação e interface permanecem genéricos.

Cada compra registra o custo em almas e materiais. O botão de reinício devolve esses valores registrados, mesmo que os custos do asset sejam rebalanceados depois. Saves antigos recebem um ledger estimado uma vez usando os custos atuais, pois eles não armazenavam o custo histórico por compra.

## Habilidades da campanha

A árvore abre com **Flash de Luz**, **Passos Livres** e **Vigor Solar**. Os níveis dessas três habilidades custam só almas. A primeira expedição garante as 6 almas do desbloqueio de Flash. Os preços seguintes crescem a cada nível até o limite de cada habilidade.

| Habilidade | Efeito | Níveis |
|---|---|---:|
| Flash de Luz | Desbloqueia o Flash. A partir do nível 2, aumenta duração e atordoamento em 5% por nível. | 9 |
| Passos Livres | +5% de velocidade de movimento por nível. | 6 |
| Vigor Solar | +1 de vida inicial e máxima por nível. | 5 |
| Área do Flash | Aumenta a abertura e o alcance do cone. | 4 |
| Célula de Luz | Aumenta o número de cargas. | 6 |
| Foco Restaurado | Reduz o tempo de recarga. | 3 |
| Onda de Repulsão | Aumenta o empurrão do Flash e repele inimigos para longe da torre. | 1 |
| Torreta de Lâminas | Usar Flash instala uma torreta por 11 s, com intervalo de 22 s. | 1 |
| Recuperação por Abate | Recarrega Flash ao derrotar elites ou chefes. | 1 |
| Dash Extra | Adiciona uma carga de dash. | 1 |
| Ímã de Cristais | Amplia em 0,75 unidade o alcance de coleta por nível. | 3 |
| Mamaco Isca | Atrai inimigos por 10 s no ponto de saída do dash, com intervalo de 18 s. | 1 |
| Último Tiro | Dobra o dano do disparo que esvazia o pente. | 1 |
| Recarga Explosiva | Iniciar a recarga com o pente vazio detona uma explosão. | 1 |
| Mira Precisa | Dá +5 pontos percentuais de crítico por nível. No nível 4, 15% dos críticos viram supercríticos. | 4 |
| Disparo Duplo | Dá 18% de chance por nível de disparar outra bala sem gastar munição extra. | 2 |
| Disparo Espalhado | Adiciona dois projéteis e consome um cartucho extra. Cada projétil causa 60% do dano normal. | 1 |
| Pistola Astral | Desbloqueia a arma por 180 almas e 4 Éter de Velária. | 1 |
| Pistola de Rajada | Desbloqueia a arma por 450 almas e 3 Cristais Frios de Nivora. | 1 |
| Lâminas Guardiãs | Uma lâmina por nível protege a torre. Cada golpe recebido remove uma lâmina. O nível 2 custa também 4 Fragmentos Umbrais de Dorsália. | 2 |
| Luz Nutrida | Aumenta em 8% o crescimento da luz por alma entregue a cada nível. A partir do nível 2, exige Éter. | 4 |
| Colheita de Almas | Aumenta em 3 pontos percentuais a chance de um inimigo deixar uma alma por nível, até 100%. Exige Cristal Frio. | 3 |

**Reembolsar** devolve o custo registrado do último nível pago da habilidade selecionada. Quando outro upgrade comprado precisa daquele nível, o painel indica qual upgrade reembolsar primeiro. Disparo Duplo e Disparo Espalhado são incompatíveis e o reembolso permite trocar de especialização depois de devolver os níveis comprados.
