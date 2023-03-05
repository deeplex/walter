<script lang="ts">
	import type { PageData } from './$types';
	import {
		WalterHeaderDetail,
		WalterGrid,
		WalterErhaltungsaufwendung
	} from '$WalterComponents';

	export let data: PageData;
</script>

<WalterHeaderDetail
	a={data.a}
	url={data.url}
	title={data.a.aussteller?.text + ' - ' + data.a.bezeichnung}
/>

<WalterGrid>
	<WalterErhaltungsaufwendung
		kontakte={data.kontakte}
		wohnungen={data.wohnungen}
		a={data.a}
	/>
</WalterGrid>
