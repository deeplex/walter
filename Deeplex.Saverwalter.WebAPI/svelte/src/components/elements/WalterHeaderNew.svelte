<script lang="ts">
    import {
        HeaderGlobalAction,
        HeaderUtilities
    } from 'carbon-components-svelte';
    import { Save } from 'carbon-icons-svelte';

    import { WalterHeader } from '$walter/components';
    import { handle_save } from './WalterHeaderNew';

    export let title = 'Neu...';
    export let apiURL: string;
    export let entry: unknown;

    function click_post(): void {
        handle_save(apiURL, entry);
    }
</script>

<WalterHeader {title}>
    <slot />
    <HeaderUtilities>
        <HeaderGlobalAction on:click={click_post} icon={Save} />
    </HeaderUtilities>
</WalterHeader>
