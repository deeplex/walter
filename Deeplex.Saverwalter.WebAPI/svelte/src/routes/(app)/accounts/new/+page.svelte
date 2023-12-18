<script lang="ts">
    import { WalterGrid, WalterHeaderNew } from '$walter/components';
    import WalterAccount from '$walter/components/details/WalterAccount.svelte';
    import { WalterAccountEntry } from '$walter/lib';
    import type { PageData } from './$types';

    const entry: Partial<WalterAccountEntry> = {};

    export let data: PageData;
</script>

<WalterHeaderNew
    apiURL={WalterAccountEntry.ApiURL}
    {entry}
    title={data.title}
/>

<WalterGrid>
    <WalterAccount {entry} fetchImpl={data.fetchImpl} />
</WalterGrid>
