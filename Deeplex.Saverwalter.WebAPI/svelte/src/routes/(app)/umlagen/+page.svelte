<script lang="ts">
    import { WalterUmlagen, WalterHeader } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeader title="Umlagen" />
<WalterUmlagen fetchImpl={data.fetchImpl} rows={data.rows} fullHeight />
