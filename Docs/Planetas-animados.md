# Planetas da seleção de missões

O conjunto foi renderizado pelo Pixel Planet Generator nativo do Shakesprite. Cada planeta tem cinco arquivos editáveis em camadas no diretório `ArtSource/Planets`, numerados de `0` (escuro) a `4` (purificado).

| Planeta | Preset | Diâmetro | Seed | Inclinação |
| --- | --- | --- | --- | --- |
| Velária | Terran Wet | 88 px | 0 | 0° |
| Nivora | Ice World | 68 px | 307 | −14° |
| Asterim | Islands | 60 px | 182 | 8° |
| Dorsália | Terran Wet, tons de âmbar e verde água | 76 px | 761 | −22° |
| Varkora | Lava World, tons de brasa | 96 px | 0 | 19° |
| Marévia | Islands, oceano turquesa | 112 px | 592 | −8° |

Todos usam **240 quadros**, **100 ms por quadro** e luz com dithering. As posições de luz X/Y seguem `(-75, -43)`, `(-24, -44)`, `(-16, -32)`, `(-16, -21)` e `(1, 2)`. Nivora e Varkora somam 9 pontos a ambos os eixos para compensar a origem diferente da luz nos respectivos presets. Dentro de cada planeta, só a posição da luz muda entre os estágios.

A seleção de missões usa os PNGs em `Assets/TINS - New Mechanics/Campaign/Resources/PlanetRotations`. Cada atlas tem 16 colunas e 15 linhas, começando no canto superior esquerdo, com transparência e filtro Point. Para substituir uma versão, reexporte o PNG com a mesma grade, duração e nome. Os arquivos editáveis ficam fora de `Assets` para evitar uma segunda importação das mesmas animações.

Com a meta atual de cinco vitórias, o jogo mostra os estágios conforme a tabela. O último estágio só aparece quando o planeta está purificado.

| Vitórias | 0 | 1 | 2 | 3–4 | 5 |
| --- | --- | --- | --- | --- | --- |
| Estágio visual | 1 | 2 | 3 | 4 | 5 |

O gerador do Shakesprite é baseado em PixelPlanets, de Deep-Fold, sob licença MIT.
