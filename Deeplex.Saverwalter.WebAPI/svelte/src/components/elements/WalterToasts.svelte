<script lang="ts">
    import { ToastNotification } from 'carbon-components-svelte';
    import { fly } from 'svelte/transition';

    import { removeToast, toasts } from '$walter/store';
    import type { WalterToast } from '$walter/types';

    function close(e: CustomEvent, index: number) {
        e.preventDefault();
        removeToast(index);
    }

    let toaster: Partial<WalterToast>[] = [];

    toasts.subscribe((value) => {
        toaster = value;
    });
</script>

<div class="toast-container">
    {#each toaster as toast, i (toast)}
        <ToastNotification
            on:close={(e) => close(e, i)}
            title={toast.title}
            kind={toast.kind}
            subtitle={toast.subtitle}
            caption={new Date().toLocaleString('de-DE')}
        />
    {/each}
</div>

<style>
    .toast-container {
        z-index: 9000;
        position: fixed;
        right: 0px;
        top: 48px;
    }
</style>
