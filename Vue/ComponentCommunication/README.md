# 父子组件传值示例（Vue 2 / Vue 3）

对应笔记：[Vue 父子组件传值](../ComponentCommunication.md)

## 目录结构

```
ComponentCommunication/
├── vue3/          # Vue 3（Vite 项目）：props / emit / v-model / defineExpose / provide-inject
└── vue2.html      # Vue 2（单 HTML + CDN，双击即可打开）：$emit / $refs / $parent / EventBus / provide-inject
```

## 运行方式

### Vue 3 示例（推荐）

依赖环境：Node.js ≥ 18，npm。

```bash
cd vue3
npm install
npm run dev
```

浏览器打开 Vite 输出的地址（默认 `http://localhost:5173`）。

演示内容（WMS 收货台场景）：

1. **props（父→子）**：父组件把单据号 `orderNo` 传给子组件 `ScannerInput`。
2. **emit（子→父）**：子组件提交条码时 `emit('scan', barcode)`，父组件 `@scan` 监听并加入列表。
3. **v-model（双向）**：`v-model="currentSku"` 绑定输入框，`defineModel` 实现。
4. **ref + defineExpose（父调子）**：父组件 `ref` 拿到子组件实例，调用其 `clearInput()`。
5. **provide/inject（祖→孙）**：父组件 `provide` 仓库信息，孙组件 `WarehouseBadge` 直接 `inject`。

建议动手改：

- 把子组件里 `defineExpose({ clearInput })` 注释掉，再点"ref 调用子组件方法"看报错——体会 `script setup` 默认不暴露实例。
- 给 `ScannerInput` 加第二个 `defineModel('status')`，父组件用 `v-model:status` 绑定，体验多 v-model。

### Vue 2 示例

依赖环境：任意浏览器 + 网络（页面通过 CDN 加载 Vue 2.7）。

直接用浏览器打开 `vue2.html` 即可（或在项目目录运行 `python3 -m http.server 8000` 后访问 `http://localhost:8000/vue2.html`）。

演示内容（同一 WMS 收货台场景）：

1. **props（父→子）** + **$emit（子→父）** + **v-model**：同 Vue 3，只是写法是 Options API（`value` 属性 + `$emit('input')`）。
2. **$refs（父调子）**：父组件 `$refs.station.clearInput()`。
3. **$parent（子访问父）**：子组件 `this.$parent.addLog(...)`（注释标注：不推荐）。
4. **provide/inject（祖→孙）**：`receive-station` provide，孙组件 `warehouse-badge` inject。
5. **事件总线 EventBus（兄弟间）**：`new Vue()` 当广播站，A 发 B 收。
