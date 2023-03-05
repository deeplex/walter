<script lang="ts">
	import {
		WalterDatePicker,
		WalterNumberInput,
		WalterTextInput
	} from '$WalterComponents';
	import { Row } from 'carbon-components-svelte';
	import type { WalterZaehlerstandEntry } from '$WalterTypes';

	export let a: Partial<WalterZaehlerstandEntry> = {};
</script>

<Row>
	<WalterNumberInput bind:value={a.stand} label="Zählerstand" />
	<WalterDatePicker bind:value={a.datum} labelText="Ablesedatum" />
	<!-- TODO add einheit? -->
</Row>
<Row>
	<WalterTextInput labelText="Notiz" bind:value={a.notiz} />
</Row>
