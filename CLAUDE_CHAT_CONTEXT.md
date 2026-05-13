# Contexto do Projeto — Para o Claude Chat (UI no Godot)

## Sobre o Projeto
Jogo roguelike de deck-building em português, inspirado em Slay the Spire.
Engine: **Godot 4.6 com C# (Mono)**.
Resolução: **1920x1080, fullscreen**.

Os scripts C# já foram escritos. Você precisa criar as **cenas (.tscn) no editor do Godot**
seguindo exatamente os nomes de nós que os scripts esperam via `GetNode<>()`.

---

## Arquitetura Geral

```
[Autoloads sempre ativos]
  MouseTracker   — rastreia mouse
  UI             — CanvasLayer com FadeIn/FadeOut e AddUI()
  PlayerManager  — referência ao Player da run atual

[Fluxo de cenas]
  MainMenu.tscn
    → (Play) → CharacterSelect.tscn
      → (Confirmar) → GameManager.tscn  (cena principal do jogo)
```

---

## 1. CENA: MainMenu.tscn
**Caminho:** `res://src/Core/Menu/MainMenu.tscn`
**Script:** `src/Core/Menu/MainMenu.cs`

### Estrutura de nós obrigatória
```
MainMenu (Control — FullRect, script: MainMenu.cs)
└── Background (Control — FullRect)
    ├── BackgroundTexture (TextureRect)
    │     size_flags: Fill+Expand, stretch_mode: Cover
    │     [coloque a imagem de fundo aqui, ex: Background_Fogueira.png]
    ├── GradientOverlay (ColorRect)
    │     color: Color(0.05, 0.02, 0.12, 0.85)
    │     anchors: FullRect
    ├── TitleGroup (Control — centralizado no topo)
    │   ├── GameTitle (Label)
    │   │     text: "PROJECT HAIL MARY"
    │   │     font_size: 72, horizontal_alignment: Center
    │   └── Subtitle (Label)
    │         text: "DATASTAGE"
    │         font_size: 36, horizontal_alignment: Center
    └── ButtonsGroup (VBoxContainer — centralizado)
          separation: 20
          ├── PlayButton (Button)
          │     text: "JOGAR"
          │     [conectar signal: pressed → OnPlayPressed()]
          └── QuitButton (Button)
                text: "SAIR"
                [conectar signal: pressed → OnQuitPressed()]
```

### Como configurar no Godot
1. Crie nova cena → nó raiz `Control` → renomeie para `MainMenu`
2. Adicione o script `src/Core/Menu/MainMenu.cs`
3. Crie a hierarquia de nós conforme acima
4. Coloque `MainMenu` como cena principal:
   **Project → Project Settings → Application → Run → Main Scene**
5. Conecte os botões:
   - PlayButton: `pressed` → `MainMenu` → `OnPlayPressed()`
   - QuitButton: `pressed` → `MainMenu` → `OnQuitPressed()`

### Background Dinâmico
O script anima automaticamente o `GradientOverlay` com loop de cores pulsantes.
Para partículas extras: adicione `GPUParticles2D` como filho de `Background`
e configure o material de partículas com movimento ascendente lento.

---

## 2. CENA: CharacterSelect.tscn
**Caminho:** `res://src/Core/CharacterSelect/CharacterSelect.tscn`
**Script:** `src/Core/CharacterSelect/CharacterSelect.cs`

### Estrutura de nós obrigatória
```
CharacterSelect (Control — FullRect, script: CharacterSelect.cs)
└── Background (TextureRect — FullRect, cover)
    └── Panel (PanelContainer — centralizado, ~1400x700)
        └── VBox (VBoxContainer — separation: 30)
            ├── PanelTitle (Label)
            │     text: "Escolha seu Personagem"
            │     font_size: 48, align: Center
            ├── CharactersContainer (HBoxContainer)
            │     alignment: Center, separation: 40
            │     [o script preenche com botões de personagem aqui]
            ├── InfoPanel (PanelContainer)
            │   └── InfoVBox (VBoxContainer — separation: 12)
            │       ├── Portrait (TextureRect — 200x300, centralizado)
            │       ├── Name (Label — font_size: 32)
            │       ├── Description (Label — font_size: 18, autowrap)
            │       └── Stats (HBoxContainer — separation: 30)
            │           ├── HP (Label — font_size: 20)
            │           ├── Energy (Label — font_size: 20)
            │           └── Gold (Label — font_size: 20)
            ├── ConfirmButton (Button)
            │     text: "CONFIRMAR"
            │     [conectar: pressed → OnConfirmPressed()]
            └── BackButton (Button)
                  text: "VOLTAR"
                  [conectar: pressed → OnBackPressed()]
```

### Caminhos de nós que o script usa (GetNode)
| GetNode path | Tipo |
|---|---|
| `Background/Panel/VBox/CharactersContainer` | HBoxContainer |
| `Background/Panel/VBox/InfoPanel/Name` | Label |
| `Background/Panel/VBox/InfoPanel/Description` | Label |
| `Background/Panel/VBox/InfoPanel/Stats/HP` | Label |
| `Background/Panel/VBox/InfoPanel/Stats/Energy` | Label |
| `Background/Panel/VBox/InfoPanel/Stats/Gold` | Label |
| `Background/Panel/VBox/InfoPanel/Portrait` | TextureRect |
| `Background/Panel/VBox/ConfirmButton` | Button |
| `Background/Panel/VBox/BackButton` | Button |

### Dados de personagens
Crie a pasta `res://Data/Characters/` e adicione arquivos `.tres` do tipo `CharacterData`
(o script `src/Core/CharacterSelect/CharacterData.cs` define os campos).
Se a pasta não existir, o script usa um personagem padrão "Guerreiro" automaticamente.

---

## 3. CENA: DeckViewer.tscn
**Caminho:** `res://src/Core/Inventory/DeckViewer.tscn`
**Script:** `src/Core/Inventory/DeckViewer.cs`

### O que é
Tela de visualização do baralho com 3 modos (definidos pelo enum `ViewerMode`):
- **Inspect** — só visualiza as cartas (aberto pelo botão "Deck" na TopBar)
- **Remove** — clique em uma carta para removê-la (eventos/fogueira)
- **Upgrade** — clique em uma carta para melhorá-la (fogueira)

### Estrutura de nós obrigatória
```
DeckViewer (Control — FullRect, script: DeckViewer.cs)
└── DimBackground (ColorRect — FullRect)
      color: Color(0, 0, 0, 0.6)   ← fundo escuro semi-transparente
      └── Panel (PanelContainer — centralizado, ~1600x900)
          ├── Header (VBoxContainer — no topo do Panel)
          │   ├── Title (Label — font_size: 36, align: Center)
          │   └── InfoLabel (Label — font_size: 18, align: Center)
          ├── Scroll (ScrollContainer — expand vertical)
          │   └── Grid (GridContainer)
          │         columns: 5
          │         [o script preenche com CardDisplay aqui]
          └── CloseButton (Button)
                text: "FECHAR"
                [conectar: pressed → OnClosePressed()]
```

### Caminhos de nós que o script usa (GetNode)
| GetNode path | Tipo |
|---|---|
| `Panel/Header/Title` | Label |
| `Panel/Header/InfoLabel` | Label |
| `Panel/Scroll/Grid` | GridContainer |

### Como abrir o DeckViewer
O `GameManager.ShowDeckViewer(modo)` já está implementado.
Chame assim no código:
```csharp
// Apenas visualizar:
GameManager.Instance.ShowDeckViewer(DeckViewer.ViewerMode.Inspect);

// Para remover carta:
GameManager.Instance.ShowDeckViewer(DeckViewer.ViewerMode.Remove);

// Para melhorar carta (fogueira — já está no CampFire.cs):
GameManager.Instance.ShowDeckViewer(DeckViewer.ViewerMode.Upgrade);
```

---

## 4. BOTÃO DE DECK na TopBar (opcional mas recomendado)
Para o jogador poder ver o baralho durante a jornada, adicione um botão na cena
`src/Core/UI/TopHud.tscn` (ou `TopBar.tscn`):

1. Adicione um `Button` com texto "Deck (X)" onde X é o número de cartas
2. Conecte `pressed` a uma função que chame:
   ```csharp
   GameManager.Instance?.ShowDeckViewer(DeckViewer.ViewerMode.Inspect);
   ```

---

## 5. CENA DA LOJA — Exibição de cartas (já existe, mas como melhorar)
**Arquivo existente:** `res://src/Core/Shop/Shop.tscn`

A loja já usa `CardDisplay` para mostrar cartas. Para melhorar a visualização:

### Adicionar seção visual de cartas
A cena `Shop.tscn` deve ter esta estrutura (o script Shop.cs já espera esses nós):
```
Shop (Control)
└── Panel (PanelContainer — ~1600x800, centralizado)
    └── MarginContainer
        └── VBoxContainer
            ├── Header (HBoxContainer)
            │   ├── TitleLabel (Label — "LOJA")
            │   └── GoldLabel (Label — "Ouro: 0")
            ├── CardsLabel (Label — "Cartas disponíveis")
            ├── CardsContainer (HBoxContainer — separation: 30)
            │     [o script insere CardDisplay aqui]
            ├── RelicsLabel (Label — "Relíquias")  ← criado pelo script
            ├── RelicsContainer (HBoxContainer)    ← criado pelo script
            ├── MessageLabel (Label — centralizado)
            └── ExitButton (Button)
                  text: "SAIR DA LOJA"
                  [conectar: pressed → OnExitPressed()]
```

O script Shop.cs faz `GetNode` pelos caminhos:
- `Panel/MarginContainer/VBoxContainer/CardsContainer`
- `Panel/MarginContainer/VBoxContainer/Header/GoldLabel`
- `Panel/MarginContainer/VBoxContainer/MessageLabel`

---

## Resumo dos arquivos C# criados/modificados

| Arquivo | Status | O que faz |
|---|---|---|
| `src/Core/RunData.cs` | **NOVO** | Guarda personagem selecionado entre cenas |
| `src/Core/Menu/MainMenu.cs` | **NOVO** | Controla menu principal + animações |
| `src/Core/CharacterSelect/CharacterData.cs` | **NOVO** | Resource com dados do personagem |
| `src/Core/CharacterSelect/CharacterSelect.cs` | **NOVO** | Tela de seleção de personagem |
| `src/Core/Inventory/DeckViewer.cs` | **NOVO** | Visualizador do baralho (Inspect/Remove/Upgrade) |
| `src/Core/CardDisplay.cs` | **MODIFICADO** | Adicionado contexto `DeckViewer` ao enum |
| `src/Core/GameManager/GameManager.cs` | **MODIFICADO** | Adicionado `ShowDeckViewer()` |
| `src/Core/Player/Player.cs` | **MODIFICADO** | Usa `RunData.SelectedCharacter` se disponível |
| `src/Core/CampFire/CampFire.cs` | **MODIFICADO** | Botão Upgrade abre DeckViewer em modo Upgrade |

---

## Ordem de trabalho recomendada no Godot

1. Crie `MainMenu.tscn` → configure como cena principal
2. Crie `CharacterSelect.tscn`
3. Crie `DeckViewer.tscn`
4. (Opcional) Melhore a UI da `Shop.tscn` com o layout acima
5. (Opcional) Adicione botão "Deck" na TopBar/TopHud

---

## Dicas importantes de Godot 4

- Para centralizar um `PanelContainer`: use **Anchor Preset = Center** e ajuste tamanho manualmente
- Para um nó cobrir toda a tela: **Anchor Preset = Full Rect**
- Para conectar um sinal de botão: clique no botão → Node tab → `pressed` → duplo clique → selecione o nó com a função
- Para fazer um `Label` com quebra de linha automática: `Autowrap Mode = Word`
- Para o `ScrollContainer` funcionar bem: coloque `size_flags` do filho como `Fill + Expand`
- O `GridContainer` com `columns: 5` organiza as cartas em grade automaticamente
