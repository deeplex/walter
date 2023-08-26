<script lang="ts">
    import { Content } from 'carbon-components-svelte';
    import { WalterHeader } from '$walter/components';
    import type { PageData } from './$types';
    import WalterMiettabelle from '$walter/components/details/WalterMiettabelle.svelte';

    export let data: PageData;


</script>

<WalterHeader title="SaverWalter" />
<Content>
    <WalterMiettabelle vertraege={data.rows}/>
</Content>
