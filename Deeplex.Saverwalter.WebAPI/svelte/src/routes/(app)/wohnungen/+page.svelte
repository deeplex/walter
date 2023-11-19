<script lang="ts">
    import { WalterHeader, WalterWohnungen } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeader title="Wohnungen" />
<WalterWohnungen fetchImpl={data.fetchImpl} rows={data.rows} fullHeight />
