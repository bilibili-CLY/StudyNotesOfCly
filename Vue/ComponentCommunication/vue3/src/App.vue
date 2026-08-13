<script setup>
import { reactive, ref, provide } from 'vue'
import ScannerInput from './components/ScannerInput.vue'

const orderNo = ref('PO-2026-0813')
const currentSku = ref('SKU-1001')
const scannedList = ref([])
const scannerRef = ref(null)

provide('warehouse', reactive({ code: 'WH-01', name: '华东一号库' }))

function onScan(barcode) {
  scannedList.value.push(barcode)
}

function callChildMethod() {
  scannerRef.value.clearInput()
}
</script>

<template>
  <main class="page">
    <h2>Vue 3 父子组件传值演示（WMS 收货台）</h2>

    <section class="card">
      <div class="card-title">
        <span>② 父组件 App</span>
        <span class="muted">
          <code>provide</code> 给整棵子树：仓库信息（reactive）
        </span>
      </div>

      <p class="muted">
        <code>props</code> 向下传单据号：
        <code class="hl">:order-no="orderNo"</code>
        <br />
        <code>v-model</code> 双向绑定当前 SKU：
        <code class="hl">v-model="currentSku"</code>
        <br />
        <code>@scan</code> 监听子组件 <code>emit</code> 上报的条码
      </p>

      <ScannerInput
        ref="scannerRef"
        :order-no="orderNo"
        v-model="currentSku"
        @scan="onScan"
      />

      <div class="row">
        <button class="btn" @click="callChildMethod">
          ref 调用子组件 clearInput()（defineExpose）
        </button>
      </div>
    </section>

    <section class="card">
      <div class="card-title"><span>③ 子组件 emit 上报的结果</span></div>
      <ul class="list">
        <li v-for="(barcode, i) in scannedList" :key="i">
          {{ barcode }}
        </li>
      </ul>
      <p v-if="!scannedList.length" class="muted">还没有扫码记录，先在上面提交一个条码</p>
    </section>
  </main>
</template>

<style scoped>
.page {
  max-width: 720px;
  margin: 0 auto;
  font-family: system-ui, sans-serif;
}
.card {
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 12px;
  margin: 12px 0;
}
.card-title {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.muted {
  color: #666;
  font-size: 13px;
}
.hl {
  background: #f1f5f9;
  padding: 0 4px;
  border-radius: 4px;
}
.row {
  margin-top: 8px;
}
.btn {
  padding: 6px 12px;
  border: 1px solid #ccc;
  border-radius: 4px;
  background: #fff;
  cursor: pointer;
}
.list li {
  font-family: monospace;
  margin: 4px 0;
}
</style>
