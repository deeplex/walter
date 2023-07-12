<script lang="ts">
    import { WalterVertraege, WalterHeaderList } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeaderList title="Verträge" />
<WalterVertraege
    fetchImpl={data.fetchImpl}
    search
    rows={data.rows}
    fullHeight
/>
