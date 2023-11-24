<script lang="ts">
    import { WalterGrid, WalterHeader } from "$walter/components";
    import WalterAccount from "$walter/components/details/WalterAccount.svelte";
    import type { PageData } from './$types';

    export let data: PageData;

    const title = `${data.entry.username} - ${data.entry.name}`
</script>

<WalterHeader {title}  />

<WalterGrid>
    <WalterAccount entry={data.entry} fetchImpl={data.fetchImpl} />
</WalterGrid>
