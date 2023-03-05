<script lang="ts">
	import { WalterAdresse, WalterTextInput } from '$WalterComponents';
	import type { WalterZaehlerEntry } from '$WalterTypes';
	import { Row } from 'carbon-components-svelte';
	import WalterComboBox from '../elements/WalterComboBox.svelte';

	export let a: Partial<WalterZaehlerEntry> = {};
</script>

<Row>
	<WalterTextInput bind:value={a.kennnummer} labelText="Kennnummer" />
	<WalterComboBox
		bind:value={a.typ}
		titleText="Typ"
		api={`/api/selection/zaehlertypen`}
	/>
</Row>
<WalterAdresse bind:value={a.adresse} />
<Row>
	<WalterComboBox
		bind:value={a.wohnung}
		titleText="Wohnung"
		api={`/api/selection/wohnungen`}
	/>
	<WalterComboBox
		bind:value={a.allgemeinZaehler}
		titleText="Allgemeinzähler"
		api={`/api/selection/zaehler`}
	/>
</Row>
<Row>
	<WalterTextInput bind:value={a.notiz} labelText="Notiz" />
</Row>
