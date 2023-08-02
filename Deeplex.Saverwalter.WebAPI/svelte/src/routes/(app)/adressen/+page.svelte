<script lang="ts">
    import { WalterAdressen, WalterHeaderList } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeaderList title="Adressen" />
<WalterAdressen rows={data.rows} fullHeight />
