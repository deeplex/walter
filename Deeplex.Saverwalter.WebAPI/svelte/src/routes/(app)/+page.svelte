<script lang="ts">
    import { Content } from 'carbon-components-svelte';
    import { WalterHeader, WalterMiettabelleWrapper } from '$walter/components';
    import type { PageData } from './$types';

    export let data: PageData;


</script>

<WalterHeader title="SaverWalter" />
<Content>
    <WalterMiettabelleWrapper
        fetchImpl={data.fetchImpl}
        umlagen={data.umlagen}
        vertraege={data.vertraege}/>
</Content>
