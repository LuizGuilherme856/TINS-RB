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

## Ramos de combate e defesa

- **Mobilidade**. Passos Livres revela Dash Extra e Ímã de Cristais. Dash Extra libera Mamaco Isca, que atrai inimigos no ponto de saída do dash por 10 segundos. Ao expirar, cada inimigo volta ao alvo anterior. O intervalo entre iscas começa ao colocá-las e dura 18 segundos.
- **Armas**. Último Tiro revela Recarga Explosiva, Mira Precisa e duas especializações incompatíveis, Disparo Duplo e Disparo Espalhado. Reiniciar a árvore permite trocar de especialização. Mira Precisa começa em 5% de crítico e chega a 20% no nível 4. Nesse nível, 15% dos críticos viram supercríticos.
- **Flash**. Área do Flash revela Onda de Repulsão e depois Torreta de Lâminas. A torreta fica no ponto de uso do Flash e dura até 11 segundos, com intervalo de 22 segundos. Célula de Luz libera Recuperação por Abate. A opção **Is Elite** na lista de inimigos do planeta define quais variantes rendem uma carga, além dos chefes que já têm essa marca.
- **Torre**. Vigor Solar revela Lâminas Guardiãs. Cada nível inicia a missão com uma lâmina em órbita da torre. Cada golpe recebido pela torre remove uma lâmina durante a missão.
