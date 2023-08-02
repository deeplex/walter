<script lang="ts">
    import { WalterAdressen, WalterHeader } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeader title="Adressen" />
<WalterAdressen rows={data.rows} fullHeight />
