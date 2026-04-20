# Unityコマンド

Unity の GameObjects、Transforms、Components を操作するためのコマンドです。

## hierarchy

シーンヒエラルキーをツリー構造で表示します。

### 書式

```bash
hierarchy [-r] [-d depth] [-a] [-l] [-s scene] [-n pattern] [-c component] [-t tag] [-y layer] [path]
```

### 説明

現在のシーンの GameObjects をツリー構造で表示します。名前、コンポーネント、タグ、またはレイヤーでフィルタリングできます。パスが指定されている場合は、そのオブジェクトの子を表示します。

### オプション

| オプション | ロング形式 | 説明 |
|--------|------|-------------|
| `-r` | `--recursive` | 子を再帰的に表示します |
| `-d` | `--depth` | 表示する最大深度 (-1 = 無制限, デフォルト) |
| `-a` | `--all` | 非アクティブなオブジェクトも含めます |
| `-l` | `--long` | 詳細情報（アクティブ状態、コンポーネント数、タグ）を表示します |
| `-s` | `--scene` | 対象のシーン名。`list` を指定するとロードされているシーンを表示します |
| `-n` | `--name` | 名前でフィルタリング（`*` や `?` ワイルドカードをサポート） |
| `-c` | `--component` | コンポーネント型でフィルタリング |
| `-t` | `--tag` | タグでフィルタリング |
| `-y` | `--layer` | レイヤー名または番号 (0-31) でフィルタリング |

### 引数

| 引数 | 説明 |
|----------|-------------|
| `path` | オプション。シーンのルートの代わりに、このオブジェクトの子を表示します。 |

### 出力形式

**通常形式:**
```
Scene: SampleScene (3 root objects)
├── Main Camera
├── Directional Light
└── Player
    ├── Model
    └── Weapon
```

**詳細形式 (`-l`):**
```
Scene: SampleScene (3 root objects)
├── [A] Main Camera              (4 components) [MainCamera]
├── [A] Directional Light        (2 components) [Untagged]
└── [A] Player                   (5 components) [Player]
```

`[A]` = アクティブ, `[-]` = 非アクティブ

### 使用例

```bash
# アクティブなシーンのルートオブジェクトを表示
hierarchy

# 再帰的に表示
hierarchy -r

# 詳細情報付きで表示
hierarchy -l

# 深度を3レベルに制限
hierarchy -r -d 3

# 非アクティブなオブジェクトも含める
hierarchy -r -a

# 名前でフィルタリング（ワイルドカード）
hierarchy -n "Player*"
hierarchy -n "*Enemy*"

# コンポーネントでフィルタリング
hierarchy -c Rigidbody
hierarchy -r -c "UnityEngine.UI.Image"

# タグでフィルタリング
hierarchy -t Player

# レイヤーでフィルタリング
hierarchy -y 5
hierarchy -y "UI"

# 特定のオブジェクトの子を表示
hierarchy /Canvas

# ロードされているシーンを一覧表示
hierarchy -s list

# 特定のシーンを対象にする
hierarchy -s "Level1"

# フィルタの組み合わせ
hierarchy -r -a -n "*Manager*" -c MonoBehaviour
```

### 終了コード

| コード | 説明 |
|------|-------------|
| 0 | 成功 |
| 1 | 無効なフィルタパターン、または不明なレイヤー/コンポーネント |
| 2 | GameObject またはシーンが見つかりません |

---

## go

GameObject を管理します（作成、削除、検索、名前変更、アクティブ化、複製、情報表示）。

### 書式

```bash
go <subcommand> [options] [arguments]
```

### サブコマンド

#### create

新しい GameObject を作成します。

```bash
go create [name] [--primitive type] [--parent path] [--position x,y,z] [--rotation x,y,z] [--tag tag]
```

| オプション | 説明 |
|--------|-------------|
| `--primitive`, `-p` | プリミティブ型: Cube, Sphere, Capsule, Cylinder, Plane, Quad |
| `--parent` | 親オブジェクトのパス |
| `--position` | 初期ワールド座標 (x,y,z) |
| `--rotation` | 初期回転角（オイラー角） (x,y,z) |
| `--tag`, `-t` | 割り当てるタグ |

**使用例:**
```bash
# 空の GameObject を作成
go create MyObject

# 名前と座標を指定して作成
go create Player --position 0,1,0

# プリミティブを作成
go create MyCube --primitive Cube

# 他のオブジェクトの子として作成
go create Child --parent /Parent

# タグを指定して作成
go create Enemy --tag Enemy
```

#### delete

GameObject を削除します。

```bash
go delete <path> [--immediate] [--children]
```

| オプション | 説明 |
|--------|-------------|
| `--immediate` | Destroy の代わりに DestroyImmediate を使用します |
| `--children` | 子オブジェクトのみを削除し、自身は残します |

**使用例:**
```bash
# オブジェクトを削除
go delete /MyObject

# 即座に削除（エディタスクリプト用）
go delete /Temp --immediate

# 子のみを削除
go delete /Parent --children
```

#### find

GameObject を検索します。

```bash
go find [-n name] [-t tag] [-c component] [-i]
```

| オプション | 説明 |
|--------|-------------|
| `-n`, `--name` | 名前パターン（部分一致、大文字小文字を区別しない） |
| `-t`, `--tag` | タグ名 |
| `-c`, `--component` | コンポーネント型 |
| `-i`, `--inactive` | 非アクティブなオブジェクトも含める |

**使用例:**
```bash
# 名前で検索
go find -n Player
go find -n "Enemy"

# タグで検索
go find -t Player

# コンポーネントで検索
go find -c Rigidbody

# 非アクティブなオブジェクトも含める
go find -n Manager -i

# フィルタの組み合わせ
go find -t Enemy -c EnemyAI
```

#### rename

GameObject の名前を変更します。

```bash
go rename <path> <new-name>
```

**使用例:**
```bash
go rename /OldName NewName
go rename /Player/Weapon Sword
```

#### active

アクティブ状態の取得または設定を行います。

```bash
go active <path> [--set true|false] [--toggle]
```

| オプション | 説明 |
|--------|-------------|
| `-s`, `--set` | アクティブ状態を設定 (true/false) |
| `--toggle` | 現在の状態を反転 |

**使用例:**
```bash
# アクティブ状態を表示
go active /MyObject

# アクティブにする
go active /MyObject --set true

# 非アクティブにする
go active /MyObject --set false

# 反転させる
go active /MyObject --toggle
```

#### clone

GameObject を複製します。

```bash
go clone <path> [-n name] [--parent path] [--count N]
```

| オプション | 説明 |
|--------|-------------|
| `-n`, `--name` | 複製後の新しい名前 |
| `--parent` | 複製の親 (デフォルト: オリジナルと同じ) |
| `--count` | 作成する複製の数 |

**使用例:**
```bash
# オブジェクトを複製
go clone /Template

# 新しい名前で複製
go clone /Enemy -n EnemyClone

# 複数複製
go clone /Bullet --count 10

# 別の親オブジェクトの下に複製
go clone /Prefab --parent /Container
```

#### info

GameObject の詳細情報を表示します。

```bash
go info <path>
```

**出力内容:**
- 名前、パス、アクティブ状態
- タグ、レイヤー、スタティックフラグ
- Transform（座標、回転、スケール）
- コンポーネント一覧
- 子オブジェクト一覧

**使用例:**
```bash
go info /Player
```

### 終了コード

| コード | 説明 |
|------|-------------|
| 0 | 成功 |
| 1 | サブコマンドの欠落、または無効な引数 |
| 2 | GameObject が見つからない、無効なプリミティブ型、または操作の失敗 |

---

## transform

GameObject の Transform を操作します。

### 書式

```bash
transform <path> [-p pos] [-P pos] [-r rot] [-R rot] [-s scale] [--parent path] [-w]
```

### 説明

Transform のプロパティを取得または設定します。オプションがない場合は、現在の Transform 情報を表示します。

### オプション

| オプション | ロング形式 | 説明 |
|--------|------|-------------|
| `-p` | `--position` | ワールド座標を設定 (x,y,z) |
| `-P` | `--local-position` | ローカル座標を設定 (x,y,z) |
| `-r` | `--rotation` | ワールド回転（オイラー角）を設定 (x,y,z) |
| `-R` | `--local-rotation` | ローカル回転（オイラー角）を設定 (x,y,z) |
| `-s` | `--scale` | ローカルスケールを設定 (x,y,z または均一スケールのための単一値) |
| | `--parent` | 親を設定 (`/`, `null`, `none` を指定すると親を解除) |
| `-w` | `--world` | 親を変更する際にワールド座標を維持する (デフォルト: true) |

### ベクトルの形式

ベクトルは以下のように指定できます：
- `x,y,z` - 3成分
- `x,y` - 2成分 (z = 0)
- `n` - 単一値 (すべての成分に適用)

### 使用例

```bash
# Transform 情報を表示
transform /Player

# ワールド座標を設定
transform /Player -p 10,0,5

# ローカル座標を設定
transform /Player -P 0,1,0

# ワールド回転を設定
transform /Player -r 0,90,0

# ローカル回転を設定
transform /Player -R 45,0,0

# 均一スケールを設定
transform /Player -s 2

# 不均一スケールを設定
transform /Player -s 1,2,1

# 親を設定
transform /Child --parent /NewParent

# 親を解除（ルートに移動）
transform /Child --parent /
transform /Child --parent null

# 複数のプロパティを設定
transform /Player -p 0,1,0 -r 0,180,0 -s 1.5,1.5,1.5
```

### 出力

**情報表示:**
```
Transform: Player
  World Position:  (10.00, 0.00, 5.00)
  Local Position:  (10.00, 0.00, 5.00)
  World Rotation:  (0.00, 90.00, 0.00)
  Local Rotation:  (0.00, 90.00, 0.00)
  Local Scale:     (1.00, 1.00, 1.00)
  Parent:          (none)
  Children:        3
  Sibling Index:   0
```

**変更時の表示:**
```
Transform: Player
  Position: (0.00, 0.00, 0.00) -> (10.00, 0.00, 5.00)
```

### 終了コード

| コード | 説明 |
|------|-------------|
| 0 | 成功 |
| 1 | 無効なベクトル形式 |
| 2 | GameObject または親が見つからない、循環参照の発生 |

---

## component

GameObject のコンポーネントを管理します。

### 書式

```bash
component <subcommand> <path> [arguments] [-a] [-v] [-i] [-n namespace]
```

### オプション (グローバル)

| オプション | ロング形式 | 説明 |
|--------|------|-------------|
| `-a` | `--all` | 一致するすべてのコンポーネントを削除 / すべてのメンバを含める |
| `-v` | `--verbose` | 完全な型名を表示 |
| `-i` | `--immediate` | 削除時に DestroyImmediate を使用 |
| `-n` | `--namespace` | 型解決のための名前空間 |

### サブコマンド

#### list

GameObject 上のコンポーネントを一覧表示します。

```bash
component list <path> [-v]
```

**出力:**
```
Components on Player (5):
  [0] Transform
  [1] Rigidbody (enabled)
  [2] CapsuleCollider (enabled)
  [3] PlayerController (enabled)
  [4] Animator (disabled)
```

**詳細出力 (`-v`):**
```
Components on Player (5):
  [0] Transform                    UnityEngine.Transform
  [1] Rigidbody                    UnityEngine.Rigidbody
  ...
```

#### add

GameObject にコンポーネントを追加します。

```bash
component add <path> <type>
```

**使用例:**
```bash
component add /Player Rigidbody
component add /Player BoxCollider
component add /Canvas "UnityEngine.UI.Image"
```

#### remove

GameObject からコンポーネントを削除します。

```bash
component remove <path> <type|index> [-a] [-i]
```

**使用例:**
```bash
# 型名で削除
component remove /Player Rigidbody

# インデックスで削除
component remove /Player 2

# その型のすべてのコンポーネントを削除
component remove /Player BoxCollider -a

# 即座に削除
component remove /Player Rigidbody --immediate
```

**注意:** Transform は削除できません。

#### info

コンポーネントの詳細情報を表示します。

```bash
component info <path> <type|index>
```

**出力:**
```
Component: Rigidbody
  Type: UnityEngine.Rigidbody
  GameObject: /Player
  Enabled: true
  Properties:
    mass: 1 (Single)
    drag: 0 (Single)
    angularDrag: 0.05 (Single)
    useGravity: true (Boolean)
    ...
```

#### enable / disable

コンポーネントの有効化または無効化を行います。

```bash
component enable <path> <type|index>
component disable <path> <type|index>
```

**使用例:**
```bash
component enable /Player Rigidbody
component disable /Player 3
```

**注意:** Behaviour、Collider、Renderer コンポーネントのみが有効化/無効化をサポートしています。

### 終了コード

| コード | 説明 |
|------|-------------|
| 0 | 成功 |
| 1 | 引数の欠落、またはインデックスが範囲外 |
| 2 | GameObject が見つからない、コンポーネント型が見つからない、追加/削除ができない |

---

## property

リフレクションを介してコンポーネントのプロパティ値を取得または設定します。

### 書式

```bash
property <subcommand> <path> <component> [property] [value] [-a] [-s] [-n namespace]
```

### オプション

| オプション | ロング形式 | 説明 |
|--------|------|-------------|
| `-a` | `--all` | プライベートフィールドも含める |
| `-s` | `--serialized` | SerializeField メンバのみを表示 |
| `-n` | `--namespace` | 解決のためのコンポーネントの名前空間 |

### サブコマンド

#### list

コンポーネントのすべてのプロパティを表示します。

```bash
property list <path> <component> [-a] [-s]
```

**出力:**
```
Properties of Rigidbody on /Player:
  mass                     float          1
  drag                     float          0
  angularDrag              float          0.05
  useGravity               bool           true
  isKinematic              bool           false
  velocity                 Vector3        (0.00, 0.00, 0.00) [readonly]
  ...
```

#### get

1つ以上のプロパティ値を取得します。

```bash
property get <path> <component> <property[,property2,...]>
```

**使用例:**
```bash
# 単一プロパティ
property get /Player Rigidbody mass

# 複数プロパティ
property get /Player Transform position,rotation,localScale

# 配列要素
property get /Renderer MeshRenderer materials[0]
```

**出力:**
```
Rigidbody.mass = 1 (float)
```

#### set

プロパティ値を設定します。

```bash
property set <path> <component> <property> <value>
```

### サポートされている値の型

| 型 | 書式 | 例 |
|------|--------|---------|
| int, float, double | 数値 | `10`, `3.14` |
| bool | true/false | `true`, `false` |
| string | テキスト | `"Hello World"` |
| Vector2 | x,y | `1.5,2.0` |
| Vector3 | x,y,z | `1,2,3` |
| Vector4 | x,y,z,w | `1,2,3,4` |
| Color | r,g,b,a (0-1) | `1,0,0,1` (赤) |
| Quaternion | x,y,z,w | `0,0,0,1` |
| Enum | 名前または値 | `ForceMode.Impulse`, `1` |
| 配列要素 | name[index] | `materials[0]` |

### 使用例

```bash
# 数値
property set /Player Rigidbody mass 10
property set /Player Rigidbody drag 0.5

# Boolean
property set /Player Rigidbody useGravity false
property set /Player Rigidbody isKinematic true

# Vector3
property set /Player Transform position 0,1,0
property set /Player Transform localScale 2,2,2

# Color (RGBA, 0-1 範囲)
property set /Sprite SpriteRenderer color 1,0,0,1

# String
property set /Text TextMesh text "Hello World"

# Enum
property set /Player Rigidbody interpolation Interpolate

# 配列要素
property set /Renderer MeshRenderer materials[0] MyMaterial
```

### 終了コード

| コード | 説明 |
|------|-------------|
| 0 | 成功 |
| 1 | 無効な値形式、または型変換の失敗 |
| 2 | GameObject、コンポーネント、またはプロパティが見つからない、またはプロパティが読み取り専用 |

---

## 実用的な例

### シーンセットアップスクリプト

```bash
# ゲーム構造の作成
go create GameManager
go create Player --primitive Capsule --position 0,1,0
go create Ground --primitive Plane
transform /Ground -s 10,1,10

# 物理の設定
component add /Player Rigidbody
component add /Player CapsuleCollider
component add /Ground MeshCollider

# プレイヤー物理の設定
property set /Player Rigidbody mass 1
property set /Player Rigidbody drag 0.5
property set /Player Rigidbody angularDrag 0.5

# 敵の作成
go create EnemySpawner
go create Enemy --primitive Cube --position 5,1,0 --tag Enemy
go clone /Enemy --count 4
```

### デバッグとインスペクション

```bash
# すべての Rigidbody を検索
hierarchy -r -c Rigidbody

# プレイヤーの状態をインスペクト
go info /Player
component list /Player -v
property list /Player Rigidbody

# Transform ヒエラルキーを確認
hierarchy /Player -r -l

# 非アクティブなオブジェクトを検索
hierarchy -r -a | grep "\[-\]"

# シーン構造をエクスポート
hierarchy -r -l > scene_structure.txt
```

### 実行時の変更

```bash
# オブジェクトの表示/非表示を切り替え
go active /UI/PauseMenu --toggle

# プレイヤー位置をリセット
transform /Player -p 0,1,0 -r 0,0,0

# すべての敵 AI を無効化
go find -t Enemy -c EnemyAI | component disable EnemyAI

# マテリアルカラーを変更
property set /Player MeshRenderer materials[0].color 0,1,0,1
```
