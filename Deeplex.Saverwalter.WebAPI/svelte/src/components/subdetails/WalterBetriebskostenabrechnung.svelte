<script lang="ts">
  import { WalterNumberInput } from '$WalterComponents';
  import { walter_s3_post } from '$WalterServices/s3';
  import { Button, Row } from 'carbon-components-svelte';

  export let f: typeof fetch;
  export let id: number;
  let jahr: number = new Date().getFullYear() - 1;

  const headers = {
    'Content-Type': 'application/octet-stream'
  };

  function click() {
    const apiURL = `/api/betriebskostenabrechnung/${id}/${jahr}`;
    fetch(apiURL, {
      method: 'GET',
      headers
    })
      .then((e) => e.blob())
      .then((e) =>
        walter_s3_post(
          new File([e], `Abrechnung ${jahr}.docx`),
          `vertraege/${id}`,
          f
        )
      );
  }
</script>

<Row>
  <WalterNumberInput bind:value={jahr} label="Jahr" hideSteppers={false} />
  <Button on:click={click}>Betriebskostenabrechnung erstellen</Button>
</Row>
