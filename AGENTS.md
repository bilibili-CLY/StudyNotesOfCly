# AGENTS.md

## Project Overview / 项目简介

本仓库是 cly 的个人编程学习笔记（综合笔记仓库），存放笔记、可运行代码示例、资源链接与踩坑记录。

- 用途：记录学习过程，便于日后回顾与检索
- 技术方向：不固定（当前以 .NET/C# 为主，含 EntityFramework、WMS 业务等）
- 内容形式：以 Markdown 笔记为主，混合总结提炼与原文摘抄

## Repository Structure / 目录结构

笔记按主题组织，每个主题一个子目录：

```
<repo>/
├── AGENTS.md          # 本文件（agent 规则）
├── README.md          # 根索引，链接各主题 README
├── TODO.md            # 学习待办列表
└── <topic>/           # 主题目录，如 DotNet/、CSharp/、Database/
    ├── README.md      # 该主题索引，列出目录内笔记
    ├── <note>.md      # 笔记文件
    └── <note>/        # 该笔记关联的可运行代码示例目录（可选）
        └── README.md  # 示例运行说明
```

- 主题目录命名使用 PascalCase（如 `DotNet/`、`EF-Core/`）
- 根 README.md 维护主题索引；每个主题目录必须有 README.md

## Note Template / 笔记模板

新建笔记套用以下固定模板（字段可省略但保留结构）：

```markdown
# <title>   # 标题

- date: YYYY-MM-DD
- tags: [C#, EF Core, WMS]
- summary: 一句话摘要

## 概述 / Overview        （用自己的话总结）
## 核心知识点 / Key Points
## 代码示例 / Code Example （指向 <note>/ 目录或内嵌片段）
## 参考链接 / References   （- [标题](url)）
## 踩坑记录 / Troubleshooting （问题、原因、解决方案）
```

## Code Examples / 代码示例

- 代码示例必须可运行
- 每个示例为最小可运行项目目录，放在笔记同名兄弟目录 `<topic>/<note>/` 下
- 示例目录必须含 `README.md`，写明依赖环境与运行命令（如 `dotnet run`、`npm start`）
- 只保留跑通核心逻辑的最小集，不做生产级结构
- 代码案例尽量取材于 WMS/ERP 中真实存在的业务流程（如收货、上架、拣货、出库、库存变动、过账、审批、导入导出），避免用无业务含义的玩具示例
- 涉及流程时尽量配流程图：在 Markdown 中用 mermaid 代码块画出业务流程图，先讲清业务流程再讲代码

## Images & Attachments / 图片与附件

- 图片存放于 `<topic>/<note>.assets/images/`（笔记内用相对路径引用）
- 引用格式：`![alt](./<note>.assets/images/xxx.png)`
- 禁止在笔记中使用本地绝对路径

## Indexing / 索引维护

- 根 README.md：维护主题列表与简介
- 主题 README.md：列出该主题下笔记文件与一句话说明
- 新增/移动笔记后，同步更新对应 README 索引

## Git Commit Convention / 提交规范

使用 Conventional Commits，描述用中文：

```
<type>: <中文描述>

feat: 新增 EF Core 底层执行流程笔记
docs: 补充 EntityFramework 踩坑记录
chore: 更新 TODO 列表
```

常用 type：`feat`（新内容）、`docs`（笔记/文档修改）、`fix`、`refactor`、`chore`、`test`。

## Do & Don't / 行为规则

### Do（应做）
- 新建笔记时套用笔记模板
- 新增/移动笔记后同步更新对应 README 索引
- 代码示例必须可运行并附运行说明
- 用相对路径引用图片与附件
- 意图不明确时先向用户提问，禁止猜测

### Don't（禁止）
- 禁止编造：不得虚构 API、示例代码或文档内容
- 未经用户明确要求，不得修改已有笔记的内容
- 未经用户明确要求，不得删除任何内容或移动文件
- 不得重写 git 提交历史（rebase / amend / force push）
- 不得提交密钥、凭据或个人信息
- 不得在笔记中写入本地绝对路径

## Tooling / 环境说明

- 仓库用 git 管理，忽略规则见 `.gitignore`（当前忽略 `.code`）
- 提交前用 `git status` / `git diff` 检查改动，再按提交规范提交
