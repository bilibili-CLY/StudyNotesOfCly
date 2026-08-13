<script setup>
import { inject } from 'vue'

const warehouse = inject('warehouse')
</script>

<template>
  <span class="badge" v-if="warehouse">
    孙组件 inject 到：{{ warehouse.code }} {{ warehouse.name }}
  </span>
</template>

<style scoped>
.badge {
  font-size: 12px;
  background: #f0f9ff;
  color: #0369a1;
  border: 1px solid #bae6fd;
  border-radius: 999px;
  padding: 2px 8px;
}
</style>
