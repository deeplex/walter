<script lang="ts">
    import { WalterHeader, WalterZaehlerList } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeader title="Zähler" />
<WalterZaehlerList
    fetchImpl={data.fetchImpl}
    rows={data.rows}
    fullHeight
/>
