<script lang="ts">
    import { WalterHeaderList, WalterZaehlerList } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeaderList title="Zähler" />
<WalterZaehlerList
    fetchImpl={data.fetchImpl}
    search
    rows={data.rows}
    fullHeight
/>
