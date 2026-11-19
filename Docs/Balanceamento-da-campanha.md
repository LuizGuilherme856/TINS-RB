# Balanceamento da defesa planetária

## Primeira expedição

A meta para um jogador iniciante em Velária é uma primeira defesa de 20 a 45 segundos. A derrota pode vir da torre ou do jogador. Uma boa execução pode vencer já na primeira tentativa. No primeiro retorno à Base, o jogador já pode comprar o Flash de Luz por **6 almas**.

Cada inimigo comum derrotado durante a missão deixa uma alma. A derrota devolve 70% do total recolhido, arredondando para cima. Na primeira expedição o jogo completa o saldo que faltar para comprar o flash. Com zero coletas, o saldo chega a seis. Com dez almas coletadas, uma derrota rende sete. Reiniciar as habilidades devolve os recursos gastos e mantém a garantia da primeira missão consumida.

## Pesquisa e decisões

- Em *Rogue Legacy*, o dinheiro ganho numa tentativa difícil paga melhorias permanentes depois da morte. No TINS, a primeira compra muda a ação que o jogador pode executar na próxima defesa. [Post-mortem de Teddy e Kenny Lee, GDC](https://media.gdcvault.com/GDC2014/Presentations/Lee_Teddy_Rogue_Legacy_Design.pdf)
- George Fan mostra como o início de *Plants vs. Zombies* apresenta uma ferramenta e permite experimentar seu efeito sobre uma ameaça conhecida. Em Velária o Demoninho aparece antes e depois da compra do flash. A mudança de quatro disparos para um fica legível contra o mesmo alvo. Nivora então introduz o Morcegão. [Apresentação de George Fan, GDC](https://media.gdcvault.com/gdc2012/slides/Design%20Track/Fan_George_How%20I%20Got.pdf)
- Anthony Pecorella compara custos geométricos à geração de moeda. Para TINS, a unidade de comparação é uma defesa concluída. O custo por nível sobe 75%, e a renda acompanha as coletas e o bônus do planeta. Duração, cargas e frequência de atordoamento têm limites porque os três efeitos protegem a torre ao mesmo tempo. [The Math of Idle Games, parte I](https://www.kongregate.com/id/pages/the-math-of-idle-games-part-i)

Os números desta campanha são escolhas de design. Para avaliar a primeira compra, acompanhe tempo de sobrevivência, almas entregues e resultado. Uma vitória rápida também indica que o jogador passou a controlar a defesa.

## Pressão na primeira missão

A Simple Pistol da campanha causa 3 de dano, tem 5% de crítico, oito balas, intervalo de 0,32 s e recarga de 1,2 s. Um Demoninho começa com 12 de vida. Oito acertos comuns eliminam dois. Com mira perfeita e sem críticos, a capacidade sustentada é de 8 ÷ (8 × 0,32 + 1,2) ÷ 4 = **0,53 eliminações/s**. Velária começa com três inimigos e recebe **0,61/s**. Erros e deslocamento para coletar almas diminuem a margem.

O primeiro flash fornece três cargas. O cone atordoa por 1,5 s e um tiro finaliza cada alvo atingido durante o atordoamento. Ao esgotar as cargas, a recarga leva 5 s. O skill check permite antecipar o fim.

## Curva por planeta

Velocidade e vida multiplicam os atributos de cada inimigo do Raining Blood. O intervalo diminui conforme as entregas expandem a luz. A tabela mostra o intervalo inicial e o piso configurado. O teto limita quantos inimigos podem estar ativos ao mesmo tempo.

| Planeta | Ameaças | Iniciais / teto | Intervalo (s) | Velocidade | Vida inicial | Bônus de vitória |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| Velária | Demoninho | 3 / 15 | 1,65 → 0,85 | 1,65× | 2,40× | 22 almas |
| Nivora | Demoninho, Morcegão | 3 / 17 | 1,58 → 0,80 | 1,68× | 2,50× | 46 almas |
| Asterim | Coxinha, Demoninho, Eye | 3 / 19 | 1,50 → 0,76 | 1,70× | 2,65× | 68 almas |
| Dorsália | Brain Spider, Coxinha, Hand | 4 / 21 | 1,44 → 0,72 | 1,72× | 2,80× | 92 almas |
| Varkora | Demoninho de Fogo, Fireball, Hand | 4 / 23 | 1,38 → 0,68 | 1,74× | 3,00× | 122 almas |
| Marévia | The Undead, Brain Spider, Eye, Demoninho de Fogo | 5 / 25 | 1,32 → 0,64 | 1,76× | 3,20× | 156 almas |

A partir de Asterim o spawner também forma grupos. Asterim acrescenta um inimigo a cada quatro aparições regulares depois do raio 7. Dorsália acrescenta um a cada três depois do raio 8. Varkora acrescenta dois a cada três depois do raio 9. Marévia acrescenta dois a cada duas depois do raio 10. Cada integrante usa o elenco e os pontos de entrada existentes, respeitando o mesmo teto. Na reta final de Marévia, isso permite até 3,13 entradas/s antes de atingir o teto. Os intervalos entre grupos dão tempo para reposicionar o jogador e recarregar.

O raio inicial é 3,5. De Velária a Marévia, cada alma entregue expande o raio em 0,50, 0,45, 0,38, 0,34, 0,30 e 0,27. Os objetivos de 11, 13, 15, 17, 19 e 21 exigem respectivamente **15, 22, 31, 40, 52 e 65 entregas**. Com o bônus, vitórias com só as coletas necessárias rendem **37, 68, 99, 132, 174 e 221 almas**. Cinco vitórias liberam o planeta seguinte. Reservar as 185 almas de cinco defesas de Velária permite comprar a Pistola Astral por 180 almas e 4 Éteres. Gastar antes com cargas, recarga ou mobilidade adia a arma.

## Compras da árvore

| Habilidade | Efeito e custo |
| --- | --- |
| Flash de Luz | 6 almas por três cargas. O primeiro aumento de duração custa 10. Cada melhoria acrescenta 5% à duração do cone e do stun. A partir do nível 7 também exige Sal Estelar de Marévia. Máximo de +40% |
| Passos Livres | +5% de movimento por nível, começando em 15 almas. A partir do nível 4 exige Fragmento Umbral de Dorsália. Máximo de +30% |
| Célula de Luz | +1 carga por nível após desbloquear o flash, começando em 20 almas. A partir do nível 5 exige Núcleo Rubro de Varkora. Máximo de nove cargas no total |
| Foco Restaurado | −10% do tempo base de recarga por nível, começando em 25 almas e 1 Éter. Com três níveis, a recarga normal cai para 3,5 s |
| Vigor Solar | +1 de vida inicial e máxima por nível, começando em 30 almas e 2 Poeiras Solares de Asterim. Máximo de +3 |
| Pistola Astral | Libera a pistola laser por 180 almas e 4 Éteres |
| Pistola de Rajada | Libera a pistola automática perfurante por 450 almas e 3 Cristais Frios |

O preço em almas é o custo-base multiplicado por 1,75 elevado ao número de melhorias anteriores, arredondado para cima. O desbloqueio do flash tem preço próprio. Materiais crescem por um fator de 1,4 a partir do primeiro nível que os exige. Os três últimos níveis de duração custam 165, 288 e 503 almas, mais 4, 6 e 8 Sais Estelares.

## Mini-bosses avaliados

**Gromsch** tem 500 de vida base e libera o laser conforme perde vida. **Cthulhu** tem 1.000, alterna teleporte, escudo e raios e libera disparos radiais por fase. O flash atual permite finalizar um inimigo atordoado com um tiro. Colocar os dois diretamente na lista de aparições permitiria pular suas transformações.

Para um encontro futuro, Gromsch cabe na quinta defesa de Dorsália. O aviso antes do laser exigiria reposicionamento enquanto a torre continua sob pressão. Cthulhu cabe na quinta defesa de Marévia, onde o escudo e o teleporte mudariam a prioridade da mira. Para esses encontros vale testar o flash interrompendo um ataque ou abrindo uma janela de dano. A campanha desta versão usa os nove prefabs regulares já integrados.

## Onde ajustar no Unity

- Cada ScriptableObject em **Assets/TINS - New Mechanics/Campaign/Mission Enemies** configura o elenco e a pressão de um planeta.
- **Resources/CampaignCatalog.asset**, na pasta Campaign, configura o crescimento da luz por entrega, as recompensas dos planetas e os custos, requisitos e níveis das habilidades.
- **Resources/CampaignGuns/simple Pistol_GunData.asset** configura a pistola inicial específica da campanha.

## Resultado da verificação em Play Mode

Uma sequência automatizada usou controle virtual, movimento, mira, disparo, coleta, estação de habilidades e defesa da torre nas cenas da campanha. A primeira tentativa começou com perfil vazio. Depois da derrota, o botão do flash ficou comprável com exatamente seis almas. A vitória seguinte pagou 37 almas e 1 Éter. Para testar os planetas seguintes, o perfil recebeu as vitórias prévias e o orçamento correspondente antes de comprar equipamentos da árvore. As coletas durante as missões vieram dos próprios inimigos.

| Tentativa | Resultado | Tempo de jogo | Vida final da torre | Almas recolhidas |
| --- | --- | ---: | ---: | ---: |
| Velária, só pistola | Derrota | 27,0 s | 0/10 | 8 |
| Velária, após comprar o flash | Vitória | 30,8 s | 9/10 | 15 |
| Nivora | Vitória | 37,0 s | 9/10 | 24 |
| Asterim | Vitória | 47,8 s | 8/10 | 34 |
| Dorsália | Vitória | 55,7 s | 8/10 | 42 |
| Varkora | Vitória | 55,2 s | 6/10 | 55 |
| Marévia | Vitória | 64,3 s | 3/10 | 65 |

A verificação de economia cobriu todos os níveis, materiais insuficientes, reembolso integral e migração de um perfil com melhorias de flash anteriores. Os tempos são de partidas individuais com mira automatizada. Uma rodada com jogadores iniciantes ainda é necessária para estimar a distribuição real de tempo até a derrota e a taxa de vitórias depois da primeira compra.
