<script lang="ts">
    import {
        WalterErhaltungsaufwendungen, WalterHeader,
    } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeader title="Erhaltungsaufwendungen" />
<WalterErhaltungsaufwendungen
    fetchImpl={data.fetch}
    rows={data.rows}
    fullHeight
/>
