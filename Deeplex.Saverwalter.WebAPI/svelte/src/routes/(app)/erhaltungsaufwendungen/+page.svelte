<script lang="ts">
    import {
        WalterErhaltungsaufwendungen,
        WalterHeaderList
    } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeaderList title="Erhaltungsaufwendungen" />
<WalterErhaltungsaufwendungen
    fetchImpl={data.fetch}
    rows={data.rows}
    fullHeight
/>
