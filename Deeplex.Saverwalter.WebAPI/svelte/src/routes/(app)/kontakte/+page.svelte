<script lang="ts">
    import { WalterKontakte, WalterHeaderList } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeaderList title="Kontakte" />
<WalterKontakte search rows={data.rows} />
