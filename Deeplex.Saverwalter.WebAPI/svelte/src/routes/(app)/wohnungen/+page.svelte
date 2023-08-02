<script lang="ts">
    import { WalterHeaderList, WalterWohnungen } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeaderList title="Wohnungen" />
<WalterWohnungen
    fetchImpl={data.fetchImpl}
    rows={data.rows}
    fullHeight
/>
