<script setup>
import { ref } from 'vue'
import WarehouseBadge from './WarehouseBadge.vue'

const props = defineProps({
  orderNo: { type: String, required: true }
})

const emit = defineEmits(['scan'])

const inputValue = defineModel({ type: String, default: '' })

const inputRef = ref(null)

function submit() {
  if (!inputValue.value) return
  emit('scan', inputValue.value)
  inputValue.value = ''
}

function clearInput() {
  inputValue.value = ''
  inputRef.value?.focus()
}

defineExpose({ clearInput })
</script>

<template>
  <section class="card">
    <div class="card-title">
      <span>① 扫码输入（子组件 ScannerInput）</span>
      <WarehouseBadge />
    </div>
    <p class="muted">
      通过 <code>props</code> 接收父级单据号：
      <strong>{{ orderNo }}</strong>
    </p>
    <input
      ref="inputRef"
      v-model="inputValue"
      class="input"
      placeholder="输入或扫描 SKU 编码，回车提交"
      @keyup.enter="submit"
    />
    <div class="row">
      <button class="btn primary" @click="submit">提交（emit scan）</button>
      <button class="btn" @click="clearInput">清空（自身方法）</button>
    </div>
  </section>
</template>

<style scoped>
.card {
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 12px;
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
.input {
  width: 100%;
  box-sizing: border-box;
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
}
.row {
  margin-top: 8px;
  display: flex;
  gap: 8px;
}
.btn {
  padding: 6px 12px;
  border: 1px solid #ccc;
  border-radius: 4px;
  background: #fff;
  cursor: pointer;
}
.btn.primary {
  background: #42b883;
  color: #fff;
  border-color: #42b883;
}
</style>
