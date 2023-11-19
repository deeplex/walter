<script lang="ts">
    import { WalterVertraege, WalterHeader } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeader title="Verträge" />
<WalterVertraege fetchImpl={data.fetchImpl} rows={data.rows} fullHeight />
