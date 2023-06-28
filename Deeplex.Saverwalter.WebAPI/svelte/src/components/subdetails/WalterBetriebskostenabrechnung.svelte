<script lang="ts">
    import { WalterNumberInput } from '$walter/components';
    import { Button, Row } from 'carbon-components-svelte';
    import { post_files } from './WalterBetriebskostenabrechnung';

    export let fetchImpl: typeof fetch;
    export let id: number;
    let jahr: number = new Date().getFullYear() - 1;

    function click() {
        post_files(id, jahr, fetchImpl);
    }
</script>

<Row>
    <WalterNumberInput bind:value={jahr} label="Jahr" hideSteppers={false} />
    <Button on:click={click}>Betriebskostenabrechnung erstellen</Button>
</Row>
