<script setup lang="ts">
import {ref} from 'vue'
import { emitter } from '@/types/emitter'

const message = ref<string>('');

function raiseEvent(){
    emitter.emit('foo', message.value);
}
</script>
<template>
    <main class="v-container">
        <h1>Component A</h1>
        <input type="text" v-model="message"/>
        <button @click="raiseEvent()">Raise Event From Component A</button>
    </main>
</template>
<style scoped>

</style>