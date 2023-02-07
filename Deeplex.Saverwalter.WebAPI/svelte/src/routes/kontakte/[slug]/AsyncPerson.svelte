<script>
	import { Column, Grid, Row, TextInput, TextInputSkeleton } from 'carbon-components-svelte';
</script>

<Grid>
	<Row>
		<!-- Bezeichnung -->
		<Column><TextInputSkeleton /></Column>
	</Row>
	<Row>
		<!-- Adresse -->
		<Column><TextInputSkeleton /></Column>
		<Column><TextInputSkeleton /></Column>
		<Column><TextInputSkeleton /></Column>
		<Column><TextInputSkeleton /></Column>
	</Row>
	<Row>
		<!-- E-Mail Fax -->
		<Column><TextInputSkeleton /></Column>
		<Column><TextInputSkeleton /></Column>
	</Row>
	<Row>
		<!-- Telefon Mobil -->
		<Column><TextInputSkeleton /></Column>
		<Column><TextInputSkeleton /></Column>
	</Row>
	<Row>
		<!-- Notiz -->
		<Column><TextInputSkeleton /></Column>
	</Row>
</Grid>
