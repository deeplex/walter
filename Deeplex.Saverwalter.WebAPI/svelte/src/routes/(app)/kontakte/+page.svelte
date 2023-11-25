<script lang="ts">
    import { WalterKontakte, WalterHeader } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeader title="Kontakte" />
<WalterKontakte fetchImpl={data.fetchImpl} rows={data.rows} fullHeight />
