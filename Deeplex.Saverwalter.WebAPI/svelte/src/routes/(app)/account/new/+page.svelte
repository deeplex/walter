<script lang="ts">
    import { WalterGrid, WalterHeaderNew } from "$walter/components";
    import WalterAccount from "$walter/components/details/WalterAccount.svelte";
    import { WalterAccountEntry } from "$walter/lib";
    import type { PageData } from './$types';

    export let data: PageData;
    const title = "Neuen Nutzer anlegen";
    const entry: Partial<WalterAccountEntry> = {};

</script>

<WalterHeaderNew apiURL={WalterAccountEntry.ApiURL} {entry} {title} />

<WalterGrid>
    <WalterAccount entry={data.entry} fetchImpl={data.fetchImpl} />
</WalterGrid>
