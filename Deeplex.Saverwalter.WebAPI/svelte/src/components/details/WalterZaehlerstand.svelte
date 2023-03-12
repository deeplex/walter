<script lang="ts">
	import {
		WalterDatePicker,
		WalterNumberInput,
		WalterTextArea
	} from '$WalterComponents';
	import { Row } from 'carbon-components-svelte';
	import type { WalterZaehlerstandEntry } from '$WalterTypes';
	import WalterTextInput from '../elements/WalterTextInput.svelte';

	export let a: Partial<WalterZaehlerstandEntry> = {};
</script>

<Row>
	<WalterNumberInput bind:value={a.stand} label="Zählerstand" />
	<WalterDatePicker bind:value={a.datum} labelText="Ablesedatum" />
	<WalterTextInput readonly bind:value={a.einheit} labelText="Einehit" />
</Row>
<Row>
	<WalterTextArea labelText="Notiz" bind:value={a.notiz} />
</Row>
